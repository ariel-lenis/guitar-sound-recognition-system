#include "TsCudaANNFunctions.cuh"
#include <Windows.h>
#include <helper_functions.h>
#include <helper_cuda.h>


void printDeviceData(float* dptr,int n)
{
	float* res = new float[n];
	
	cudaMemcpy(res,dptr,n*sizeof(float),cudaMemcpyKind::cudaMemcpyDeviceToHost);

	printf("\n");
	for(int i=0;i<n;i++)
		printf("%f\t",res[i]);
	printf("\n");

	delete[] res;
}
void printDeviceDataD(double* dptr,int n)
{
	double* res = new double[n];
	
	cudaMemcpy(res,dptr,n*sizeof(double),cudaMemcpyKind::cudaMemcpyDeviceToHost);

	printf("\n");
	for(int i=0;i<n;i++)
		printf("%lf\t",res[i]);
	printf("\n");

	delete[] res;
}
void printDeviceDataD(int* dptr,int n)
{
	int* res = new int[n];
	
	cudaMemcpy(res,dptr,n*sizeof(int),cudaMemcpyKind::cudaMemcpyDeviceToHost);

	printf("\n");
	for(int i=0;i<n;i++)
		printf("%d\t",res[i]);
	printf("\n");

	delete[] res;
}
int main()
{
    int layers = 4;
	int* layerssize = new int[4];
	
	/*
	layerssize[0]=2;
	layerssize[1]=1024;
	layerssize[2]=2048;
	layerssize[3]=32;
	layerssize[4]=1;
	*/

	layerssize[0]=2048*0+2;
	layerssize[1]=2048*2*0+512;
	layerssize[2]=256;
	layerssize[3]=1;

	Network* thenetwork;
	cublasHandle_t handle;
	cublasCreate(&handle);

	StopWatchInterface *timer = 0;
	sdkCreateTimer(&timer);


	int size = 0;

	for(int i=0;i<layers;i++)
	{
		size+=layerssize[i]*5;
		if(i<layers-1)
			size+=layerssize[i]*layerssize[i+1];
	}
	size*=sizeof(float);


	printf("total %f mb\n",size/1024.0f/1024.0f);


	cudaError_t cudaStatus = hostStartNetwork(layerssize,layers,thenetwork);

	printDeviceDataD(thenetwork->device->sumsW,thenetwork->host->layers-1);

	//getchar();

    if (cudaStatus != cudaSuccess) {fprintf(stderr, "addWithCuda failed!");return 1;}

	float* inputs = new float[layerssize[0]];
	float* results = new float[1];
	float* expected = new float[1];

	for(int i=0;i<layerssize[0];i++)
		inputs[i]=0;

	*expected=0;

	inputs[0]=1;
	inputs[1]=0;
	
	sdkStartTimer(&timer);

	for(int i=0;i<100000;i++)
	{
		int x = i%4;
		inputs[0]=x/2;
		inputs[1]=x%2;
		*expected=(x==1||x==2);
		hostCudaTrain(thenetwork,handle,inputs,0.5,0.05,expected,NULL);
	}

	sdkStopTimer(&timer);
	float reduceTime = sdkGetAverageTimerValue(&timer);

	printf("Time:%f\n",reduceTime);
	
	for(int i=0;i<4;i++)
	{
		inputs[0]=i/2;
		inputs[1]=i%2;
		*results=0;

		hostCudaForward(thenetwork,handle,inputs,0.5,results,NULL);

		printf("(%f,%f)=>%f\n",inputs[0],inputs[1],results[0]);
	}	
	
	disposeNetwork(thenetwork);


	cudaStatus = cudaDeviceReset();
    if (cudaStatus != cudaSuccess) {fprintf(stderr, "cudaDeviceReset failed!");return 1;}
    return 0;
}
