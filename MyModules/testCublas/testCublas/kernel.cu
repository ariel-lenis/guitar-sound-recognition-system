#include "cuda_runtime.h"
#include "device_launch_parameters.h"
#include <cublas_v2.h>
#include <stdio.h>
#include <stdlib.h>
#include <helper_timer.h>

cudaError_t cublasMultiply(float *a, float *b, float *c, unsigned int na,unsigned int nb);

int main()
{
	float a[]={ 0.1,0.2,0.3,0.4};

	float b[]={ 1.1,1.2,1.3,
			    2.1,2.2,2.3,
				3.1,3.2,3.3,
				4.1,4.2,4.3};

	float* c = new float[3];
	//memset(c,0,sizeof(float));
	for(int i=0;i<3;i++)
		c[i]=i;

	int colsa=4;
	int rowsa=1;

	int colsb=3;
	int rowsb=4;

	float* dptra;
	float* dptrb;
	float* dptrc;

	cudaMalloc((void**)&dptra,4*sizeof(float));
	cudaMalloc((void**)&dptrb,12*sizeof(float));
	cudaMalloc((void**)&dptrc,3*sizeof(float));

	cudaMemcpy(dptra,a,4*sizeof(float),cudaMemcpyKind::cudaMemcpyHostToDevice);
	cudaMemcpy(dptrb,b,12*sizeof(float),cudaMemcpyKind::cudaMemcpyHostToDevice);
	cudaMemcpy(dptrc,c,3*sizeof(float),cudaMemcpyKind::cudaMemcpyHostToDevice);


    cublasHandle_t handle;
    cublasCreate(&handle);
    // Do the actual multiplication
	float alpha=1;
	float beta=0;
	/*
    const float alf = 1;
    const float bet = 0;
    const float *alpha = &alf;
    const float *beta = &bet;
	*/
	cublasStatus_t res = cublasSgemm(handle, CUBLAS_OP_N, CUBLAS_OP_T, 1,3, 4, &alpha, dptra, 1, dptrb, 3, &beta, dptrc, 1);
	//cublasStatus_t res = cublasSgemm(handle, CUBLAS_OP_N, CUBLAS_OP_N, 1,4, 3, &alpha, dptra, 1, dptrb, 4, &beta, dptrc, 1);
    // Destroy the handle
    cublasDestroy(handle);
	
	printf("%d\n",res);

	cudaMemcpy(c,dptrc,3*sizeof(float),cudaMemcpyKind::cudaMemcpyDeviceToHost);

	for(int i=0;i<3;i++)
	{
		printf("%f ",c[i]);
	}


	delete[] c;
    cudaDeviceReset();
    return 0;
}
void gpu_blas_mmul(const float *A, const float *B, float *C, unsigned int m, unsigned int k, unsigned int n) {
    int lda=m,ldb=k,ldc=m;
    const float alf = 1;
    const float bet = 0;
    const float *alpha = &alf;
    const float *beta = &bet;


    // Create a handle for CUBLAS
    cublasHandle_t handle;
    cublasCreate(&handle);
    // Do the actual multiplication
    cublasSgemm(handle, CUBLAS_OP_N, CUBLAS_OP_N, m, n, k, alpha, A, lda, B, ldb, beta, C, ldc);
    // Destroy the handle
    cublasDestroy(handle);
}
// Helper function for using CUDA to add vectors in parallel.
cudaError_t cublasMultiply(float *a, float *b, float *c, unsigned int na,unsigned int nb)
{
    float *dev_a = 0;
    float *dev_b = 0;
    float *dev_c = 0;
    cudaError_t cudaStatus;


    cudaStatus = cudaSetDevice(0);
    cudaStatus = cudaMalloc((void**)&dev_c, nb * sizeof(float));
    cudaStatus = cudaMalloc((void**)&dev_a, na * sizeof(float));
    cudaStatus = cudaMalloc((void**)&dev_b, na*nb * sizeof(float));

    cudaStatus = cudaMemcpy(dev_a, a, na * sizeof(float), cudaMemcpyHostToDevice);
    cudaStatus = cudaMemcpy(dev_b, b, nb*na * sizeof(float), cudaMemcpyHostToDevice);

	//gpu_blas_mmul(dev_a, dev_b, dev_c, nr_rows_A, nr_cols_A, nr_cols_B);
	gpu_blas_mmul(dev_a, dev_b, dev_c, 1, na, nb);

    // Check for any errors launching the kernel
    cudaStatus = cudaGetLastError();
    if (cudaStatus != cudaSuccess) {
        fprintf(stderr, "addKernel launch failed: %s\n", cudaGetErrorString(cudaStatus));
        goto Error;
    }
    
    cudaStatus = cudaDeviceSynchronize();
    if (cudaStatus != cudaSuccess) {
        fprintf(stderr, "cudaDeviceSynchronize returned error code %d after launching addKernel!\n", cudaStatus);
        goto Error;
    }

    cudaStatus = cudaMemcpy(c, dev_c, nb * sizeof(float), cudaMemcpyDeviceToHost);
    if (cudaStatus != cudaSuccess) {
        fprintf(stderr, "cudaMemcpy failed!");
        goto Error;
    }

Error:
    cudaFree(dev_c);
    cudaFree(dev_a);
    cudaFree(dev_b);
    
    return cudaStatus;
}
