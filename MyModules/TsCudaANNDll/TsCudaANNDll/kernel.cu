#include "../../TsCudaANN/TsCudaANN/TsCudaANNFunctions.cuh"

struct TheData
{
	Network* thenetwork;
	cublasHandle_t cublash;
};

TheData* WINAPI cudaANNCreateNetwork(int* layerssize,int layers)
{
	Network* thenetwork;
	cublasHandle_t cublash;

	if(hostStartNetwork(layerssize,layers,thenetwork)!=cudaError::cudaSuccess) return NULL;
	cublasCreate(&cublash);

	TheData* thedata = new TheData();
	thedata->thenetwork=thenetwork;
	thedata->cublash=cublash;

	return thedata;
}

bool WINAPI cudaANNTrain(TheData* thedata,float* inputs,float* expected,float alpha,float learningrate,float &totalerror) 
{
	int outputsize = thedata->thenetwork->host->sizesN[thedata->thenetwork->host->layers-1];
	float* errors = new float[outputsize];
	if(hostCudaTrain(thedata->thenetwork,thedata->cublash,inputs,alpha,learningrate,expected,errors)!=cudaError::cudaSuccess)
		return false;
	totalerror=0;
	for(int i=0;i<outputsize;i++)
		totalerror+=errors[i]*errors[i];
	totalerror*=0.5;
	return true;
}

bool WINAPI cudaANNForward(TheData* thedata,float* inputs,float* outputs,float alpha)
{
	return hostCudaForward(thedata->thenetwork,thedata->cublash,inputs,alpha,outputs,NULL)==cudaError::cudaSuccess;
}

bool WINAPI cudaANNFree(TheData* thedata)
{
	cublasDestroy(thedata->cublash);
	cudaFreeHost(thedata->thenetwork);
	delete thedata;
	return true;
}

bool WINAPI cudaANNBackup(TheData* thedata,char* &ptr,int &size)
{
	Network* thenetwork = thedata->thenetwork;
	int totalw = thenetwork->host->totalw;
	int totaln = thenetwork->host->totaln;	
	int layers = thenetwork->host->layers;

	size = (1 + layers + totalw + totaln)*sizeof(int) + (totaln + totalw)*sizeof(float);
	
	ptr = (char*)malloc(size);
	char* iptr = ptr;

	*((int*)iptr) = layers; iptr+=sizeof(int);

	for (int i = 0; i < layers; i++)
	{
		*((int*)iptr) = thenetwork->host->sizesN[i];
		iptr += sizeof(int);
	}

	*((int*)iptr) = totaln; iptr += sizeof(int);
	cudaMemcpy(iptr, thenetwork->host->bias[0], totaln*sizeof(float), cudaMemcpyKind::cudaMemcpyDeviceToHost);
	iptr += totaln*sizeof(float);

	*((int*)iptr) = totalw; iptr += sizeof(int);
	cudaMemcpy(iptr, thenetwork->host->weights[0], totalw*sizeof(float), cudaMemcpyKind::cudaMemcpyDeviceToHost);
	iptr += totalw*sizeof(float);

	return true;
}

bool WINAPI cudaANNRestore(TheData* thedata, char* weights, char* bias)
{
	int totalw=thedata->thenetwork->host->totalw;
	int totaln=thedata->thenetwork->host->totaln;

	cudaMemcpy(thedata->thenetwork->host->bias[0], bias, totaln*sizeof(float), cudaMemcpyKind::cudaMemcpyHostToDevice);
	cudaMemcpy(thedata->thenetwork->host->weights[0], weights, totalw*sizeof(float), cudaMemcpyKind::cudaMemcpyHostToDevice);

	return true;
}