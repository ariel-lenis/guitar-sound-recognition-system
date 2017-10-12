#include "cuda_runtime.h"
#include "device_launch_parameters.h"
#include <stdio.h>
#include <cublas_v2.h>
#include <device_functions.h>
#include <curand.h>
#include <windows.h>



#define MAX_THREADS_PER_BLOCK 512
#define MAX_BLOCKS_PER_DIMENSION 65535

struct NetworkData
{
	//Layers Number, must be allocated both Host and Device
	int layers;
	//Total Number of weights unified vector, must be allocated into both Host and Device
	int totalw;
	//Total Number of neurons unified vector, must be allocated into both Host and Device
	int totaln;
	//Neurons Number in every layer, must be allocated a copy into both Host and Device
	int* sizesN;
	//Weights Number between layers, must be allocated a copy into both Host and Device
	int* sizesW;
	//Acumulative vector of lengths of sizesW array from 0 to n-1 adding from the beggining, must be allocated a copy into both Host and Device
	int* sumsW;

	//Mask for the weights... store information about the layer that correspond to every weight, must be allocated a copy into both Host and Device
	unsigned short** masksW;

	//Vector(Matrix) of weights, must be allocated just into Device, but the pointers must be allocated on Host
	float** weights;
	//Vector(Matrix) of lastInputSums, no need no be allocated
	float** lastInputSums;
	//Vector(Matrix) of lastdActivationOutputs, no need no be allocated
	float** lastdActivationOutputs;
	//Vector(Matrix) of lastActivationOuputs, no need no be allocated
	float** lastActivationOutputs;
	//Vector(Matrix) of biases, must be allocated just into Device, but the pointers must be allocated on Host
	float** bias;
	//Vector(Matrix) of lastLocalGradients, no need no be allocated
	float** lastLocalGradients;	
	//Vector of errors, no need to be allocated
	float* errors;
	//Vector of expectedbuffer, o need to be allocated
	float* expectedbuffer;

	NetworkData* ptr;
};

struct Network
{
	NetworkData* host;
	NetworkData* device;
};

cudaError_t hostStartNetwork(int* neuronsPerLayer,int n,Network* &thenetwork);
cudaError_t disposeNetwork(Network* thenetwork);
cudaError_t hostCudaForward(Network* thenetwork,cublasHandle_t cublasHandle,float* inputs,float alpha,float* result,float* expectedoutputs);
cudaError_t hostCudaTrain(Network* thenetwork,cublasHandle_t cublasHandle,float* inputs,float alpha,float learningRate,float* outputs);


__global__ void deviceSetValue(float* ptr,float val,int n)
{
	int idx = blockIdx.y*gridDim.x*blockDim.x+blockDim.x*blockIdx.x+threadIdx.x;
	if(idx>=n) return;	
	ptr[idx]=val;
}

__global__ void deviceAfterRandom(float* ptr,int n)
{
	int idx = blockIdx.y*gridDim.x*blockDim.x+blockDim.x*blockIdx.x+threadIdx.x;
	if(idx>=n) return;	
	ptr[idx]=(ptr[idx]-0.5f)*1.5f;
}


//calculate the optimal number of threads and blocks two of then on 1 dimension
void blocksAndThreads(int n,int maxthreads,int &blocks,int &threads)
{	
	blocks = (n+MAX_THREADS_PER_BLOCK-1)/MAX_THREADS_PER_BLOCK;
	threads= (n<MAX_THREADS_PER_BLOCK)?n:MAX_THREADS_PER_BLOCK;
}
//the limit of blocks per dimension is 2^16-1, so we need to use a 2d block
void blocksAndThreads(int n,int maxthreads,dim3 &blocks,int &threads)
{	
	int _blocks = (n+MAX_THREADS_PER_BLOCK-1)/MAX_THREADS_PER_BLOCK;
	threads= (n<MAX_THREADS_PER_BLOCK)?n:MAX_THREADS_PER_BLOCK;

	int _blocks2=1;
	if(_blocks>MAX_BLOCKS_PER_DIMENSION)
	{
		_blocks2=(_blocks+MAX_BLOCKS_PER_DIMENSION-1)/MAX_BLOCKS_PER_DIMENSION;
		_blocks=MAX_BLOCKS_PER_DIMENSION;
	}
	blocks=dim3(_blocks,_blocks2);
}
//sigmoidal activation
__device__ float dSigmoidalActivation(float sigmoid,float alpha)
{
	return sigmoid*(1-sigmoid);
}
//sigmoidal first derivative based on a calculated sigmoidal activation
__device__ float sigmoidalActivation(float x,float alpha)
{
	float expp = exp(-alpha * x);	
	float res =  (float)((1.0 / (1 + expp)));
	return res;
}
//this function update the activate functions and the error if is the last layer
__global__ void deviceCudaActivateFunctions(void* ptrnetwork,int layer,float alpha,bool calculateerror)
{
	unsigned int idx = blockDim.x*blockIdx.x+threadIdx.x;
	NetworkData* network=(NetworkData*)ptrnetwork;
	if(idx>=network->sizesN[layer]) return;

	float bias = network->bias[layer][idx];
	network->lastInputSums[layer][idx]+=bias;
	float sum = network->lastInputSums[layer][idx];	
	float activation = sigmoidalActivation(sum,alpha);
	float dactivation=dSigmoidalActivation(activation,alpha);
	network->lastActivationOutputs[layer][idx]=activation;
	network->lastdActivationOutputs[layer][idx]=dactivation;	

	if(calculateerror)
	{
		float error = network->expectedbuffer[idx]-network->lastActivationOutputs[layer][idx];
		network->errors[idx]=error;
		network->lastLocalGradients[layer][idx]=error*dactivation;
	}
}
cudaError_t hostCudaStartVector(float** &pdevice,float** &phost,int* sizes,int nsizes,int sumsizes)
{
	cudaError_t cudaStatus;
	float* start=0;
	//printf("ns:%d ss:%d\n",nsizes,sumsizes);
	cudaStatus = cudaMalloc((void**)&start,sumsizes * sizeof(float));
	if(cudaStatus!=cudaError::cudaSuccess)
		printf("Error allocating data.");

	cudaStatus = cudaMalloc((void**)&pdevice,nsizes * sizeof(float*));
	if(cudaStatus!=cudaError::cudaSuccess)
		printf("Error allocating data.");

	phost=new float*[nsizes];
	int idx=0;
	for(int i=0;i<nsizes;i++)
	{
		phost[i]=&start[idx];
		idx+=sizes[i];
	}
	cudaStatus = cudaMemcpy(pdevice, phost,nsizes*sizeof(float*), cudaMemcpyHostToDevice);
	return cudaError_t::cudaSuccess;
}
cudaError_t hostCudaStartVector(unsigned short** &pdevice,unsigned short** &phost,int* sizes,int nsizes,int sumsizes)
{
	cudaError_t cudaStatus;
	unsigned short* start=0;
	//printf("ns:%d ss:%d\n",nsizes,sumsizes);
	cudaStatus = cudaMalloc((void**)&start,sumsizes * sizeof(unsigned short));
	if(cudaStatus!=cudaError::cudaSuccess)
		printf("Error allocating data.");

	cudaStatus = cudaMalloc((void**)&pdevice,nsizes * sizeof(unsigned short*));
	if(cudaStatus!=cudaError::cudaSuccess)
		printf("Error allocating data.");

	phost=new unsigned short*[nsizes];
	int idx=0;
	for(int i=0;i<nsizes;i++)
	{
		phost[i]=&start[idx];
		idx+=sizes[i];
	}
	cudaStatus = cudaMemcpy(pdevice, phost,nsizes*sizeof(unsigned short*), cudaMemcpyHostToDevice);
	return cudaError_t::cudaSuccess;
}
cudaError_t hostCudaForward(Network* thenetwork,cublasHandle_t cublasHandle,float* inputs,float alpha,float* result,float* expectedoutputs)
{
	cudaError_t cudaStatus;
	NetworkData* device = thenetwork->device;
	NetworkData* host = thenetwork->host;

	//cudaStatus=cudaError_t::cudaSuccess;

	
	cudaStatus = cudaMemcpy(host->lastActivationOutputs[0], inputs,host->sizesN[0]*sizeof(float), cudaMemcpyHostToDevice);
	if(expectedoutputs!=NULL)
		cudaStatus = cudaMemcpy(device->expectedbuffer, expectedoutputs,host->sizesN[host->layers-1]*sizeof(float), cudaMemcpyHostToDevice);
	
	int blocks,threads;

	float _alpha=1;
	float _beta=0;
	for(int i=0;i<host->layers-1;i++)
	{
		cublasStatus_t cublasresult=
		cublasSgemm(cublasHandle, CUBLAS_OP_N, CUBLAS_OP_N, 
			1,//m
			host->sizesN[i+1],//n 
			host->sizesN[i],//k 
			&_alpha,
			host->lastActivationOutputs[i],	1,//A,lda
			host->weights[i], host->sizesN[i],//B,ldb 
			&_beta, 
			host->lastInputSums[i+1],1);//C,ldc

		if(cublasresult!=cublasStatus_t::CUBLAS_STATUS_SUCCESS)
		{			
			printf("Cublas Error: %d\n",cublasresult);
		}

		blocksAndThreads(host->sizesN[i+1],MAX_THREADS_PER_BLOCK,blocks,threads);
		deviceCudaActivateFunctions<<<blocks,threads>>>(device->ptr,i+1,alpha, (expectedoutputs!=NULL && (i+1)==(host->layers-1)) );
		cudaError_t cudaStatus = cudaGetLastError();
		if (cudaStatus != cudaSuccess) {
			fprintf(stderr, "Host Forward Error: %s\n", cudaGetErrorString(cudaStatus));        
		}

	}
	if(result!=NULL)
		cudaStatus = cudaMemcpy(result,host->lastActivationOutputs[host->layers-1], host->sizesN[host->layers-1]*sizeof(float), cudaMemcpyDeviceToHost);

	return cudaStatus;
}

__global__  void cudaMultiply(float* a,float* b,int n)
{
	int idx=blockDim.x*blockIdx.x+threadIdx.x;
	if(idx>=n) return;
	a[idx]*=b[idx];
}
cudaError_t hostCudaErrorGradientLocal(Network* thenetwork,cublasHandle_t cublasHandle)
{
	float alpha=1;
	float beta=0;
	int threads,blocks;
	//the error of the output layer is already calculated...
	for(int i=thenetwork->host->layers-2;i>0;i--)
	{
		cublasStatus_t cublasresult = cublasSgemm(cublasHandle, CUBLAS_OP_N,CUBLAS_OP_T,
			1,//m
			thenetwork->host->sizesN[i],//n
			thenetwork->host->sizesN[i+1],//k 
			&alpha,
			thenetwork->host->lastLocalGradients[i+1], 1, //A,lda
			thenetwork->host->weights[i], thenetwork->host->sizesN[i], //B,ldb
			&beta, 
			thenetwork->host->lastLocalGradients[i], 1);//C,ldc
		if(cublasresult!=cublasStatus_t::CUBLAS_STATUS_SUCCESS)
		{			
			printf("Cublas Error: %d\n",cublasresult);
		}
		blocksAndThreads(thenetwork->host->sizesN[i],MAX_THREADS_PER_BLOCK,blocks,threads);
		cudaMultiply<<<blocks,threads>>>(thenetwork->host->lastLocalGradients[i],thenetwork->host->lastdActivationOutputs[i],thenetwork->host->sizesN[i]);
	}
	return cudaError::cudaSuccess;
}

__global__ void deviceCudaUpdateWeightsOld(void* ptr,int totalw,int layers,float learningrate)
{	
	__shared__ int wsums[32];
	__shared__ int nsizes[32];
	__shared__ int sum;
	__shared__ int _layers;
	
	NetworkData* thenetwork=(NetworkData*)ptr;

	int idx=blockIdx.y*gridDim.x*blockDim.x+blockDim.x*blockIdx.x+threadIdx.x;	

	if(idx>=totalw) return;
	
	if(threadIdx.x==0)
	{
		for(int i=0;i<layers-1;i++)
		{
			if(i<layers-1)
				wsums[i]=thenetwork->sumsW[i];
			nsizes[i]=thenetwork->sizesN[i];
		}
	} 	
	
	__syncthreads();

	//return;

	int fromlayer=0; for(fromlayer=0;wsums[fromlayer]<=idx;fromlayer++);

	//return;

	int wpos=idx;
	if(fromlayer>0)	wpos=wpos-wsums[fromlayer-1];

	int toNeuron=wpos/nsizes[fromlayer];
	int fromNeuron=wpos%nsizes[fromlayer];

	float delta = thenetwork->lastActivationOutputs[fromlayer][fromNeuron]*thenetwork->lastLocalGradients[fromlayer+1][toNeuron]*learningrate;
	thenetwork->weights[fromlayer][wpos]+=delta;

	//bias update just one time... so if fromneuron is 0
	if(fromNeuron==0)
	{
		delta=thenetwork->lastLocalGradients[fromlayer+1][toNeuron]*learningrate*1;//1 of bias ;)
		thenetwork->bias[fromlayer+1][toNeuron]+=delta;
	}
}

__global__ void deviceCudaUpdateWeights(void* ptr,int totalw,int layers,float learningrate)
{	
	NetworkData* thenetwork=(NetworkData*)ptr;

	int idx=blockIdx.y*gridDim.x*blockDim.x+blockDim.x*blockIdx.x+threadIdx.x;	

	if(idx>=totalw) return;	

	int fromlayer=thenetwork->masksW[0][idx];

	int wpos=idx;
	if(fromlayer>0)	wpos=wpos-thenetwork->sumsW[fromlayer-1];

	int currentSizesN = thenetwork->sizesN[fromlayer];
	int toNeuron=wpos/currentSizesN;
	int fromNeuron=wpos%currentSizesN;

	float delta = thenetwork->lastActivationOutputs[fromlayer][fromNeuron]*thenetwork->lastLocalGradients[fromlayer+1][toNeuron]*learningrate;
	thenetwork->weights[fromlayer][wpos]+=delta;

	//bias update just one time... so if fromneuron is 0
	if(fromNeuron==0)
	{
		delta=thenetwork->lastLocalGradients[fromlayer+1][toNeuron]*learningrate*1;//1 of bias ;)
		thenetwork->bias[fromlayer+1][toNeuron]+=delta;
	}
}

__global__ void deviceFillMaskWeights(void* ptr,int totalw,int layers)
{
	__shared__ int wsums[32];
	__shared__ int nsizes[32];
	__shared__ int sum;
	__shared__ int _layers;
	
	NetworkData* thenetwork=(NetworkData*)ptr;

	int idx=blockIdx.y*gridDim.x*blockDim.x+blockDim.x*blockIdx.x+threadIdx.x;	

	if(idx>=totalw) return;
	
	if(threadIdx.x==0)
	{
		for(int i=0;i<layers-1;i++)
		{
			if(i<layers-1)
				wsums[i]=thenetwork->sumsW[i];
			nsizes[i]=thenetwork->sizesN[i];
		}
	} 	
	
	__syncthreads();

	unsigned short fromlayer=0; for(fromlayer=0;wsums[fromlayer]<=idx;fromlayer++);	
	thenetwork->masksW[0][idx]=fromlayer;
}

cudaError_t hostCudaUpdateWeightsOld(Network* thenetwork,cublasHandle_t cublasHandle,float learningRate)
{
	int threads;
	dim3 blocks;
	blocksAndThreads(thenetwork->host->totalw,MAX_THREADS_PER_BLOCK,blocks,threads);
	deviceCudaUpdateWeights<<<blocks,threads>>>(thenetwork->device->ptr,thenetwork->host->totalw,thenetwork->host->layers,learningRate);
	cudaError_t cudaStatus = cudaGetLastError();
    if (cudaStatus != cudaSuccess) {
        fprintf(stderr, "Update Weights error: %s\n", cudaGetErrorString(cudaStatus));        
    }
	return cudaError::cudaSuccess;
}
__global__ void deviceCudaUpdateBias(void* ptr,float learningrate, int n)
{
	int idx = blockDim.x*blockIdx.x+threadIdx.x;
	if(idx>=n) return;
	NetworkData* network = (NetworkData*)ptr;
	network->bias[0][idx]+=network->lastLocalGradients[0][idx]*learningrate;
}
cudaError_t hostCudaUpdateWeights(Network* thenetwork,cublasHandle_t cublasHandle,float learningRate)
{
	NetworkData* host = thenetwork->host;
	NetworkData* device = thenetwork->device;

	float _alpha=learningRate;
	float _beta=1;
	for(int i=0;i<host->layers-1;i++)
	{
		cublasStatus_t cublasresult=
		cublasSgemm(cublasHandle, CUBLAS_OP_N, CUBLAS_OP_N, 
			host->sizesN[i],//m
			host->sizesN[i+1],//n 
			1,//k 
			&_alpha,
			host->lastActivationOutputs[i],	host->sizesN[i],//A,lda
			host->lastLocalGradients[i+1], 1,//B,ldb 
			&_beta, 
			host->weights[i],host->sizesN[i]);//C,ldc

		if(cublasresult!=cublasStatus_t::CUBLAS_STATUS_SUCCESS)
		{			
			printf("Cublas Error: %d\n",cublasresult);
			return cudaError::cudaErrorUnknown;
		}
	}

	int blocks,threads;		
	blocksAndThreads(host->totaln,MAX_THREADS_PER_BLOCK,blocks,threads);
	deviceCudaUpdateBias<<<blocks,threads>>>(device->ptr, learningRate, host->totaln);
	cudaError_t cudaStatus = cudaGetLastError();
	if (cudaStatus != cudaSuccess) {
		fprintf(stderr, "Host Forward Error: %s\n", cudaGetErrorString(cudaStatus));     
		return cudaStatus;
	}

	return cudaError::cudaSuccess;
}

cudaError_t hostFillMaskWeights(Network* thenetwork)
{
	int threads;
	dim3 blocks;
	blocksAndThreads(thenetwork->host->totalw,MAX_THREADS_PER_BLOCK,blocks,threads);
	deviceFillMaskWeights<<<blocks,threads>>>(thenetwork->device->ptr,thenetwork->host->totalw,thenetwork->host->layers);
	cudaError_t cudaStatus = cudaGetLastError();
    if (cudaStatus != cudaSuccess) {
        fprintf(stderr, "Filling Weights Masks Error: %s\n", cudaGetErrorString(cudaStatus));        
    }
	return cudaError::cudaSuccess;
}
cudaError_t hostCudaTrain(Network* thenetwork,cublasHandle_t cublasHandle,float* inputs,float alpha,float learningRate,float* outputs,float* errors)
{
	hostCudaForward(thenetwork,cublasHandle,inputs,alpha,NULL,outputs);
	hostCudaErrorGradientLocal(thenetwork,cublasHandle);
	hostCudaUpdateWeights(thenetwork,cublasHandle,learningRate);
	cudaDeviceSynchronize();
	if(errors!=NULL)
		cudaMemcpy(errors,thenetwork->device->errors,thenetwork->host->sizesN[thenetwork->host->layers-1]*sizeof(float),cudaMemcpyKind::cudaMemcpyDeviceToHost);
	return cudaError::cudaSuccess;
}

__global__ void cudaStartNetwork(void* ptrnetwork)
{
	int idx = blockDim.x*blockIdx.x+threadIdx.x;

	NetworkData* thenetwork = (NetworkData*)ptrnetwork;

	if(idx<thenetwork->totaln)
	{
		//structure
		//Data:****************************************************
		//	   0              1       2         3        4      5 
		//So... are correlative vectors :)

		thenetwork->lastInputSums[0][idx]=0;
		thenetwork->lastActivationOutputs[0][idx]=0;
		thenetwork->lastdActivationOutputs[0][idx]=0;
		thenetwork->lastLocalGradients[0][idx]=0;

		//Bias and weights must be updated outside
		//thenetwork->bias[0][idx]=0.5;
		
		if(idx<thenetwork->sizesN[thenetwork->layers-1])
		{
			thenetwork->errors[idx]=0;
			thenetwork->expectedbuffer[idx]=0;
		}
	}
	//Bias and weights must be updated outside
	/*
	if(idx<thenetwork->totalw)
		thenetwork->weights[0][idx]=0.5;	
	*/
}
cudaError_t hostStartNetwork(int* neuronsPerLayer,int n,Network* &thenetwork)
{
	thenetwork = new Network();
	NetworkData* device= new NetworkData();
	NetworkData* host=new NetworkData();
	
	thenetwork->device=device;
	thenetwork->host=host;
	host->ptr=host;

	host->sizesW=new int[n-1];
	host->sumsW=new int[n-1];
	host->sizesN=new int[n];	

	int* ptrsizesw;
	int totaln=0;
	int totalw=0;
	int delta;
	int blocks;
	int threads;
	cudaError_t cudaStatus;
	cudaStatus = cudaSetDevice(0);

	for(int i=0;i<n;i++)
	{
		if(i<n-1)
		{
			delta= neuronsPerLayer[i]*neuronsPerLayer[i+1];
			host->sizesW[i]=delta;
			totalw+=delta;
			host->sumsW[i]=totalw;
		}
		totaln+=neuronsPerLayer[i];
		host->sizesN[i]=neuronsPerLayer[i];
	}
	
	hostCudaStartVector(device->weights,host->weights,host->sizesW,n-1,totalw);
	hostCudaStartVector(device->masksW,host->masksW,host->sizesW,n-1,totalw);
	hostCudaStartVector(device->lastInputSums,host->lastInputSums,host->sizesN,n,totaln);
	hostCudaStartVector(device->lastActivationOutputs,host->lastActivationOutputs,host->sizesN,n,totaln);
	hostCudaStartVector(device->lastdActivationOutputs,host->lastdActivationOutputs,host->sizesN,n,totaln);
	hostCudaStartVector(device->bias,host->bias,host->sizesN,n,totaln);
	hostCudaStartVector(device->lastLocalGradients,host->lastLocalGradients,host->sizesN,n,totaln);
	
	cudaStatus = cudaMalloc((void**)&device->sizesN, n * sizeof(int));
	cudaStatus = cudaMalloc((void**)&device->sizesW, n * sizeof(int));
	cudaStatus = cudaMalloc((void**)&device->sumsW, (n-1) * sizeof(int));
	cudaStatus = cudaMalloc((void**)&device->ptr, sizeof(NetworkData));
	cudaStatus = cudaMalloc((void**)&device->errors, neuronsPerLayer[n-1] * sizeof(float*));
	cudaStatus = cudaMalloc((void**)&device->expectedbuffer, neuronsPerLayer[n-1] * sizeof(float*));
	
	//printf("%d %d %d\n",host->sumsW[0],host->sumsW[1],host->sumsW[2]);

	host->layers=device->layers=n;
	host->totalw=device->totalw=totalw;
	host->totaln=device->totaln=totaln;
	host->errors=device->errors;
	
	cudaStatus = cudaMemcpy(device->sizesN, host->sizesN, n * sizeof(int), cudaMemcpyHostToDevice);
	cudaStatus = cudaMemcpy(device->sizesW, host->sizesW,(n-1) * sizeof(int), cudaMemcpyHostToDevice);
	cudaStatus = cudaMemcpy(device->sumsW, host->sumsW,(n-1) * sizeof(int), cudaMemcpyHostToDevice);
	cudaStatus = cudaMemcpy(device->ptr, device,sizeof(NetworkData), cudaMemcpyHostToDevice);
	
	int max=totaln;

	blocksAndThreads(max,MAX_THREADS_PER_BLOCK,blocks,threads);
	cudaStartNetwork<<<blocks, threads>>>(device->ptr);

	cudaStatus = cudaGetLastError();
    if (cudaStatus != cudaSuccess) {
        fprintf(stderr, "addKernel launch failed xD: %s\n", cudaGetErrorString(cudaStatus));        
    }

	
	curandGenerator_t gen;
	curandCreateGenerator(&gen,curandRngType::CURAND_RNG_PSEUDO_DEFAULT);
	curandSetPseudoRandomGeneratorSeed(gen,GetTickCount());
	curandGenerateUniform(gen,host->weights[0],(UINT32)host->totalw);
	curandGenerateUniform(gen,host->bias[0],host->totaln);
	curandDestroyGenerator(gen);
	
	deviceAfterRandom<<<blocks,threads>>>(host->bias[0],host->totaln);	

	dim3 blocks2;
	blocksAndThreads(host->totalw,MAX_THREADS_PER_BLOCK,blocks2,threads);
	deviceAfterRandom<<<blocks2,threads>>>(host->weights[0],host->totalw);


	hostFillMaskWeights(thenetwork);
	
	/*
	deviceSetValue<<<blocks,threads>>>(host->bias[0],-0.5,host->totaln);	

	dim3 blocks2;
	blocksAndThreads(host->totalw,MAX_THREADS_PER_BLOCK,blocks2,threads);
	deviceSetValue<<<blocks2,threads>>>(host->weights[0],-0.5,host->totalw);
	*/

	cudaStatus = cudaGetLastError();
    if (cudaStatus != cudaSuccess) {
        fprintf(stderr, "addKernel launch failed: %s\n", cudaGetErrorString(cudaStatus));        
    }
    cudaStatus = cudaDeviceSynchronize();
    if (cudaStatus != cudaSuccess) {
        fprintf(stderr, "cudaDeviceSynchronize returned error code %d after launching addKernel!\n", cudaStatus);
    }

	return cudaError_t::cudaSuccess;
}

cudaError_t disposeNetwork(Network* network)
{
	cudaFree(network->device->sizesN);
	cudaFree(network->device->sizesW);
	cudaFree(network->device->sumsW);

	cudaFree(network->host->weights[0]);

	cudaFree(network->host->masksW[0]);

	cudaFree(network->host->lastInputSums[0]);
	cudaFree(network->host->lastActivationOutputs[0]);
	cudaFree(network->host->lastdActivationOutputs[0]);
	cudaFree(network->host->bias[0]);
	cudaFree(network->host->lastLocalGradients[0]);
	cudaFree(network->device->errors);
	cudaFree(network->device->expectedbuffer);
	cudaFree(network->device->ptr);
	
	return cudaError_t::cudaSuccess;
}
