#include "cuda_runtime.h"
#include "device_launch_parameters.h"
#include <stdio.h>
#include <cufft.h>
#include "windows.h"
#include <device_functions.h>

#define MAX_THREADS_PER_BLOCK 512
#define MAX_BLOCKS_PER_DIMENSION 65535

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

bool _stdcall Test()
{
	int count=0;
	cudaGetDeviceCount(&count);
	return count>0;
}
void _stdcall FFT(void* input,void* output,int n,int direction)
{
	int* cudaData = NULL;
	cufftHandle plan;

	cudaSetDevice(0);
	cudaMalloc((void**)&cudaData, 2 * n * sizeof(float));
	cudaMemcpy(cudaData, input, 2*n*sizeof(float), cudaMemcpyHostToDevice);
	cufftPlan1d(&plan,n,CUFFT_C2C,1);

	if(direction==1)
		cufftExecC2C(plan,(cufftComplex*)cudaData,(cufftComplex*)cudaData,CUFFT_FORWARD);
	else
		cufftExecC2C(plan,(cufftComplex*)cudaData,(cufftComplex*)cudaData,CUFFT_INVERSE);

	cufftDestroy(plan);
	cudaMemcpy(output, cudaData, 2*n*sizeof(float), cudaMemcpyDeviceToHost);
	cudaFree(cudaData);
}

void _stdcall MultipleFFT(void* input,void* output,int blocksize,int nblocks,int direction)
{
	int* cudaData = NULL;
	cufftHandle plan;

	int allsize = blocksize*nblocks;

	cudaSetDevice(0);
	cudaMalloc((void**)&cudaData, 2 * allsize * sizeof(float));
	cudaMemcpy(cudaData, input, 2*allsize*sizeof(float), cudaMemcpyHostToDevice);
	cufftPlan1d(&plan,blocksize,CUFFT_C2C,nblocks);

	if(direction==1)
		cufftExecC2C(plan,(cufftComplex*)cudaData,(cufftComplex*)cudaData,CUFFT_FORWARD);
	else
		cufftExecC2C(plan,(cufftComplex*)cudaData,(cufftComplex*)cudaData,CUFFT_INVERSE);

	cufftDestroy(plan);
	cudaMemcpy(output, cudaData, 2*allsize*sizeof(float), cudaMemcpyDeviceToHost);
	cudaFree(cudaData);
}

/*
__global__ void startWaveA(float* ptr,int n,int target)
{
	int i = threadIdx.x+blockIdx.x*blockDim.x;	
	if(i>=target) return;
	if(i<n/2)
	{
		ptr[2*(n-1-i)]=ptr[n-1-i];
		ptr[n-1-i]=0;
	}
	ptr[2*(n-i)-1]=ptr[i];	
	ptr[i]=0;
}
__global__ void startWaveB(float* ptr,int n,int target)
{
	int i = threadIdx.x+blockIdx.x*blockDim.x;	
	if(i>=target) return;
	ptr[i*2]=ptr[2*(n-i)-1];
	ptr[2*(n-i)-1]=0;
}

float* ptr = NULL;
int n;
float** sptr = NULL;
int srows;

bool _stdcall SetWave(float* data,int _n)
{
	if(ptr!=NULL) cudaFree(ptr);
	n=_n;

	float* ptr=NULL;
	if(cudaSetDevice(0)!=cudaSuccess) return false;
	if(cudaMalloc((void**)&ptr,2*n*sizeof(float))!=cudaSuccess) return false;

	if(cudaMemcpy(ptr, data, n*sizeof(float), cudaMemcpyHostToDevice)!=cudaSuccess)
	{
		cudaFree(ptr);
		return false;
	}

	int target = (n+1)/2;
	int blocks = (target+MAX_THREADS_PER_BLOCK-1)/MAX_THREADS_PER_BLOCK;

	startWaveA<<<blocks,MAX_THREADS_PER_BLOCK>>>(ptr,n,target);
	startWaveB<<<blocks,MAX_THREADS_PER_BLOCK>>>(ptr,n,target);
	//cudaFree(ptr);

	return true;
}
*/
/*
float* ptr = NULL;
int n;
float** sptr = NULL;
int srows;
float* buffer;
*/


float* _stdcall HostCloneWaveToDevice(float* data,int n)
{
	float *ptr;
	if(cudaSetDevice(0)!=cudaSuccess) return NULL;
	if(cudaMalloc((void**)&ptr,n*sizeof(float))!=cudaSuccess) return false;

	if(cudaMemcpy(ptr, data, n*sizeof(float), cudaMemcpyHostToDevice)!=cudaSuccess)
	{
		cudaFree(ptr);
		return NULL;
	}

	return ptr;
}

bool _stdcall HostFastFourierTransform(float* deviceData,int n,int direction)
{
	//int* cudaData = NULL;
	cufftHandle plan;
	cudaSetDevice(0);

	cufftPlan1d(&plan,n,CUFFT_C2C,1);

	if(direction==1)
	{
		if(cufftExecC2C(plan,(cufftComplex*)deviceData,(cufftComplex*)deviceData,CUFFT_FORWARD)!=cufftResult_t::CUFFT_SUCCESS)
			goto error;
	}
	else
	{
		if(cufftExecC2C(plan,(cufftComplex*)deviceData,(cufftComplex*)deviceData,CUFFT_INVERSE)!=cufftResult_t::CUFFT_SUCCESS)
			goto error;
	}

	cufftDestroy(plan);
	return true;
error:
	cufftDestroy(plan);
	//cudaFree(cudaData);	
}

__global__ void deviceMultiply(float* where,float* a,float* b,unsigned int n)
{
	int idx = blockDim.x*blockIdx.x+threadIdx.x;
	if(idx>=n) return;
	//where[idx]=a[idx]*b[idx];

	float va = a[2*idx];
	float vb = a[2*idx+1];

	float vc = b[2*idx];
	float vd = b[2*idx+1];


	where[2*idx]=va*vc-vb*vd;
	where[2*idx+1]=va*vd+vb*vc;

}
__device__ const float pi = (float)3.14159265358979323846;

__device__ const double multiplier = 1.8827925275534296252520792527491;
__device__ float FTWavelet( float x, float scale, float f0 )
{
    if ( x < 0.9 / scale ||  x > 1.1 / scale ) {
        return (float)0.0;
    }

	double two_pi_f0 = 2.0 * pi * f0;


    scale *= (float)f0;

    // 1.88279*exp(-0.5*(2*pi*x*10-2*pi*10)^2)

    float basic = (float)(multiplier *
            exp(-0.5*(2*pi*x*scale-two_pi_f0)*(2*pi*x*scale-two_pi_f0)));

    // pi^0.25*sqrt(2.0)*exp(-0.5*(2*pi*x*scale-2*pi*0.849)^2)
    return sqrt(scale)*basic;
}


__global__ void deviceModule(float* where,float* from,unsigned int n)
{
	int idx = blockDim.x*blockIdx.x+threadIdx.x;
	if(idx>=n) return;
	float dx = from[idx*2];
	float dy = from[idx*2+1];
	where[idx]=log(sqrt(dx*dx+dy*dy));
}

bool _stdcall HostSemiConvolution(cufftHandle cuplan,float* fftdeviceA,float* fftdeviceB,float* devicebuffer,float* deviceresult,unsigned int n)
{
	int blocks,threads;
	cudaError_t cudaStatus;
	blocksAndThreads(n,MAX_THREADS_PER_BLOCK,blocks,threads);
	deviceMultiply<<<blocks,threads>>>(devicebuffer,fftdeviceA,fftdeviceB,n);

	cudaStatus = cudaGetLastError();
	if (cudaStatus != cudaSuccess) goto error;
    cudaStatus = cudaDeviceSynchronize();
    if (cudaStatus != cudaSuccess) goto error;

	if(cufftExecC2C(cuplan,(cufftComplex*)devicebuffer,(cufftComplex*)devicebuffer,CUFFT_INVERSE)!=cufftResult_t::CUFFT_SUCCESS)
		goto error;
	//if(!HostFastFourierTransform(devicebuffer,n,-1)) goto error;

	blocksAndThreads(n,MAX_THREADS_PER_BLOCK,blocks,threads);
	deviceModule<<<blocks,threads>>>(deviceresult,devicebuffer,n);	
	
	cudaStatus = cudaGetLastError();
	if (cudaStatus != cudaSuccess) goto error;
    cudaStatus = cudaDeviceSynchronize();
    if (cudaStatus != cudaSuccess) goto error;

	return true;
error:
	return false;
}
__global__ void deviceRealToComplex(float* where,float* from,unsigned int n)
{
	int idx = blockDim.x*blockIdx.x+threadIdx.x;
	if(idx>=n) return;
	where[2*idx]=from[idx];
	where[2*idx+1]=0;
}
__global__ void deviceZeroMemory(float* where,unsigned int n)
{
	int idx = blockDim.x*blockIdx.x+threadIdx.x;
	if(idx>=n) return;
	where[idx]=0;
}

struct ScalogramPlan
{
	float* fftdevicea;
	float* fftdeviceb;
	float* fftdevicebuffer;
	float* fftdeviceresult;	
	cufftHandle cuplan;
	int n;
};

ScalogramPlan* WINAPI HostPlanScalogram(float* deviceData,int n)
{
	ScalogramPlan* plan = new ScalogramPlan();
	cudaError_t cudaStatus;

	float* fftdevicea=NULL;
	float* fftdeviceb=NULL;
	float* fftdevicebuffer=NULL;
	float* fftdeviceresult=NULL;
	cufftHandle cuplan=NULL;

	if(cudaMalloc((void**)&fftdevicea,2*n*sizeof(float))!=cudaSuccess) goto error;		
	if(cudaMalloc((void**)&fftdeviceb,2*n*sizeof(float))!=cudaSuccess) goto error;
	if(cudaMalloc((void**)&fftdevicebuffer,2*n*sizeof(float))!=cudaSuccess) goto error;
	if(cudaMalloc((void**)&fftdeviceresult,n*sizeof(float))!=cudaSuccess) goto error;

	int blocks,threads;
	blocksAndThreads(n,MAX_THREADS_PER_BLOCK,blocks,threads);
	deviceRealToComplex<<<blocks,threads>>>(fftdevicea,deviceData,n);

	cudaStatus = cudaGetLastError();
	if (cudaStatus != cudaSuccess) goto error;

    cudaStatus = cudaDeviceSynchronize();
    if (cudaStatus != cudaSuccess) goto error;

	blocksAndThreads(2*n,MAX_THREADS_PER_BLOCK,blocks,threads);
	deviceZeroMemory<<<blocks,threads>>>(fftdeviceb,2*n);

	cudaStatus = cudaGetLastError();
	if (cudaStatus != cudaSuccess) goto error;

    cudaStatus = cudaDeviceSynchronize();
    if (cudaStatus != cudaSuccess) goto error;	

	cufftPlan1d(&cuplan,n,CUFFT_C2C,1);

	cufftPlan1d(&cuplan,n,CUFFT_C2C,1);
	if(cufftExecC2C(cuplan,(cufftComplex*)fftdevicea,(cufftComplex*)fftdevicea,CUFFT_FORWARD)!=cufftResult_t::CUFFT_SUCCESS) goto error;

	plan->fftdevicea=fftdevicea;
	plan->fftdeviceb=fftdeviceb;
	plan->fftdevicebuffer=fftdevicebuffer;
	plan->fftdeviceresult=fftdeviceresult;
	plan->cuplan=cuplan;
	plan->n=n;


	return plan;
error:
	MessageBoxA(NULL,cudaGetErrorString(cudaStatus),NULL,0);
	if(fftdevicea!=NULL)		cudaFree(fftdevicea);
	if(fftdeviceb!=NULL)		cudaFree(fftdeviceb);
	if(fftdevicebuffer!=NULL)	cudaFree(fftdevicebuffer);
	if(fftdeviceresult!=NULL)	cudaFree(fftdeviceresult);
	return NULL;
}


__device__ float Morlet(int i,int n)
{
    float range = 4;
    float x=-range+2.0f*range*i/n;
	float amplitude=1;
    return (float)(amplitude * (exp(-x*x/2)*cos(5*x)));
}

__global__ void devicePrepareWindow(float* where,unsigned int n,unsigned int wn)
{
	int idx = blockDim.x*blockIdx.x+threadIdx.x;
	if(idx>=n) return;
	
	if(idx<wn)	where[2*idx]= Morlet(idx,wn);//window[idx];
	else		where[2*idx]=0;
	where[2*idx+1]=0;
}

/*
__global__ void devicePrepareWindow(float* where,float* window,unsigned int n,unsigned int wn)
{
	int idx = blockDim.x*blockIdx.x+threadIdx.x;
	if(idx>=n) return;
	if(idx>n/2)
	{
		where[2*idx]=0;
		where[2*idx+1]=0;	
	}
	else
	{
		float f0=82;
		where[2*idx]=FTWavelet(idx, (float)0.1*wn/n, f0 );
		where[2*idx+1]=0;
	}
}
*/
int WINAPI HostIterateWindow(int wn,ScalogramPlan* plan,float* hostresult)
{
	int blocks,threads;
	cudaError_t cudaStatus=cudaError_t::cudaSuccess;
	float* deviceWindow;
	
	cudaGetLastError();
	
	//cudaMalloc((void**)&deviceWindow,wn*sizeof(float));
	//cudaMemcpy(deviceWindow,window,wn*sizeof(float),cudaMemcpyKind::cudaMemcpyHostToDevice);

	blocksAndThreads(plan->n,MAX_THREADS_PER_BLOCK,blocks,threads);
	devicePrepareWindow<<<blocks,threads>>>(plan->fftdeviceb,plan->n,wn);
	
	cudaStatus = cudaGetLastError();
	if (cudaStatus != cudaSuccess) goto error;
    cudaStatus = cudaDeviceSynchronize();
    if (cudaStatus != cudaSuccess) goto error;

	//cudaFree(deviceWindow);


	if(cufftExecC2C(plan->cuplan,(cufftComplex*)plan->fftdeviceb,(cufftComplex*)plan->fftdeviceb,CUFFT_FORWARD)!=cufftResult_t::CUFFT_SUCCESS)
		goto error;

	//if(!HostFastFourierTransform(plan->fftdeviceb,plan->n,1)) goto error;


	if(!HostSemiConvolution(plan->cuplan,plan->fftdevicea,plan->fftdeviceb,plan->fftdevicebuffer,plan->fftdeviceresult,plan->n)) goto error;
	
	if(cudaMemcpy(hostresult,plan->fftdeviceresult,plan->n*sizeof(float),cudaMemcpyKind::cudaMemcpyDeviceToHost)!=cudaError_t::cudaSuccess)
		goto error;
	return 1;

error:
	return 0;
}


void WINAPI HostDestroyScalogram(ScalogramPlan* data)
{
	cudaFree(data->fftdevicea);
	cudaFree(data->fftdeviceb);
	cudaFree(data->fftdevicebuffer);
	cudaFree(data->fftdeviceresult);
	cufftDestroy(data->cuplan);
}



__global__ void	prepareSpectrogram(float* data,int datasize,float* buffer,int buffersize,float* cuwindow,int fftsize,int samplesrequired,float p)
{
	int id = blockIdx.x*blockDim.x+threadIdx.x;
	if(id>=buffersize) return;
	int peakpos = id/fftsize;
	int epos =id%fftsize;

	if((peakpos==0 && epos<=fftsize/2) || (peakpos==samplesrequired-1 && epos>=fftsize/2))
	{
		buffer[id*2] = 0;
		buffer[id*2+1]=0;		
		return;
	}

	int pos = (int)(peakpos*p)-fftsize/2+epos;

	if(pos<0||pos>=datasize) return;
	buffer[id*2] = data[pos]*cuwindow[epos];
	buffer[id*2+1]=0;
}
__global__ void complexToReal(float* buffer,float* buffermodule,int finalsize,int fftsize)
{
	int id = blockIdx.x*blockDim.x+threadIdx.x;
	if(id>=finalsize) return;

	int partsize = (fftsize+1)/2;
	int partid = id/partsize;
	int fftid = id%partsize;

	int pos = partid*fftsize*2+fftid*2;

	float dr = buffer[pos]*buffer[pos];
	float di = buffer[pos+1]*buffer[pos+1];
	buffermodule[id]=sqrtf(dr*dr+di*di);	
}
__global__ void cudaMean(float* xbuffer,unsigned int jump,unsigned int n)
{
	unsigned int id = blockIdx.x*blockDim.x+threadIdx.x;

	if(id*jump*2<n)
	{
		//xbuffer[id*jump*2]=xbuffer[id*jump*2];
		if(id*jump*2+jump<n)
			xbuffer[id*jump*2]+=xbuffer[id*jump*2+jump];
		if(jump==1) xbuffer[id*jump*2]/=n;
	}
}
__global__ void cudaLog(float* xbuffer,unsigned int n)
{
	unsigned int id = blockIdx.x*blockDim.x+threadIdx.x;

	if(id>=n) return;
	xbuffer[id]=logf(xbuffer[id]*10);
	xbuffer[id]+=10;
	if(xbuffer[id]<0) xbuffer[id]=0;
}
__global__ void cudaStd(float* xbuffer,float mean,unsigned int n)
{
	unsigned int id = blockIdx.x*blockDim.x+threadIdx.x;
	if(id>=n) return;
	float delta = xbuffer[id]-mean;
	xbuffer[id]=delta*delta;
}
__global__ void cudaNormalize(float* buffer,float mean,float std,int n)
{
	unsigned int id = blockIdx.x*blockDim.x+threadIdx.x;
	if(id>=n) return;

	float val = buffer[id];
	if(val>mean+std) val=mean+std;
	else if(val<mean-std) val=mean-std;

	//val=(val-mean)/std;

	//if(val<0) buffer[id]=0;

	float logk =1;
	if(std==0)	buffer[id]=0;
	else
		buffer[id] = (float)((logf(logk + val)-logf(logk)) / (logf(logk + mean+std)-logf(logk)));
}
//void STE()
float Mean(float* cuBuffer,int n,bool readonly)
{
	float* xbuffer=NULL;

	cudaSetDevice(0);

	if(readonly)
	{
		if(cudaMalloc((void**)&xbuffer,n*sizeof(float))!=cudaSuccess) goto error;		
		if(cudaMemcpy( xbuffer, cuBuffer, n*sizeof(float), cudaMemcpyDeviceToDevice)!=cudaSuccess) goto error;
	}
	else
		xbuffer=cuBuffer;
	
	int pow = 0,spow = 1;
	while(spow<n) {pow++;spow*=2;}

	int xn=n;
	unsigned int jump=1;
	for(int i=0;i<pow;i++)
	{
		xn=(xn+1)/2;
		int blocks = (xn+MAX_THREADS_PER_BLOCK-1)/MAX_THREADS_PER_BLOCK;
		cudaMean<<<blocks,MAX_THREADS_PER_BLOCK>>>(xbuffer,jump,(unsigned int)n);
		jump*=2;
	}

	float result = 0;
	if(cudaMemcpy( &result, xbuffer, sizeof(float), cudaMemcpyDeviceToHost)!=cudaSuccess) goto error;

	if(readonly)
		cudaFree(xbuffer);

	return result;
error:
	if(xbuffer!=NULL)	cudaFree(xbuffer);
	return 0;
}

float StandardDeviation(float* cuBuffer,int n,float* _mean)
{
	float* xbuffer=NULL;

	cudaSetDevice(0);	

	if(cudaMalloc((void**)&xbuffer,n*sizeof(float))!=cudaSuccess) goto error;		
	if(cudaMemcpy( xbuffer, cuBuffer, n*sizeof(float), cudaMemcpyDeviceToDevice)!=cudaSuccess) goto error;

	int blocks;

	float mean = Mean(xbuffer,n,false);

	if(cudaMemcpy( xbuffer, cuBuffer, n*sizeof(float), cudaMemcpyDeviceToDevice)!=cudaSuccess) goto error;

	blocks = (n+MAX_THREADS_PER_BLOCK-1)/MAX_THREADS_PER_BLOCK;
	cudaStd<<<blocks,MAX_THREADS_PER_BLOCK>>>(xbuffer,mean,(unsigned int)n);
	
	float std2 = Mean(xbuffer,n,false);
	*_mean=mean;

	cudaFree(xbuffer);
	
	return sqrt(std2);
error:
	if(xbuffer!=NULL)	cudaFree(xbuffer);
	return 0;
}


float* _stdcall Spectrogram(float* ptr,int n,int fftsize,int samplesrequired,float* window,float* output,float* mean,float* std)
{
	float* sptr=NULL;
	float p = (float)n/(samplesrequired-1);
	float* buffer=NULL;
	float* buffermodule=NULL;
	float* cuwindow=NULL;
	int buffersize = samplesrequired*fftsize;//but in complex the real size is 2*buffersize

	cudaSetDevice(0);
	if(cudaMalloc((void**)&buffer,2*buffersize*sizeof(float))!=cudaSuccess) goto error;		
	if(cudaMalloc((void**)&cuwindow,fftsize*sizeof(float))!=cudaSuccess) goto error;
	if(cudaMemcpy(cuwindow, window, fftsize*sizeof(float), cudaMemcpyHostToDevice)!=cudaSuccess) goto error;

	int blocks = (buffersize+MAX_THREADS_PER_BLOCK-1)/MAX_THREADS_PER_BLOCK;
	prepareSpectrogram<<<blocks,MAX_THREADS_PER_BLOCK>>>(ptr,n,buffer,buffersize,cuwindow,fftsize,samplesrequired,p);
	cudaFree(cuwindow);

	cufftHandle plan;

	int allsize = fftsize*samplesrequired;
	
	cufftPlan1d(&plan,fftsize,CUFFT_C2C,samplesrequired);
	if(cufftExecC2C(plan,(cufftComplex*)buffer,(cufftComplex*)buffer,CUFFT_FORWARD)!=cufftResult_t::CUFFT_SUCCESS) goto error;
	cufftDestroy(plan);

	int finalsize = ((fftsize+1)/2)*samplesrequired;//aprox. just the half of the fft have the data because nquyst teorem
	if(cudaMalloc((void**)&buffermodule,finalsize*sizeof(float))!=cudaSuccess) goto error;	
	
	blocks = (finalsize+MAX_THREADS_PER_BLOCK-1)/MAX_THREADS_PER_BLOCK;
	complexToReal<<<blocks,MAX_THREADS_PER_BLOCK>>>(buffer,buffermodule,finalsize,fftsize);
	cudaFree(buffer);

	//*mean = Mean(buffermodule,finalsize,true);

	*std = StandardDeviation(buffermodule,finalsize,mean);


	cudaNormalize<<<blocks,MAX_THREADS_PER_BLOCK>>>(buffermodule,*mean,*std,finalsize);


	cudaMemcpy(output, buffermodule, finalsize*sizeof(float), cudaMemcpyDeviceToHost);	
	cudaFree(buffermodule);
	return sptr;
error:
	if(sptr!=NULL)	cudaFree(sptr);
	if(buffer!=NULL)	cudaFree(buffer);
	if(cuwindow!=NULL)	cudaFree(cuwindow);
	return NULL;
}