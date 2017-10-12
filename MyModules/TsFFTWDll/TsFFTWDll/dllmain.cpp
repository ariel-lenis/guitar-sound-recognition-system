#include "stdafx.h"
#include "fftw3.h"

void _stdcall FFT(fftwf_complex* in,fftwf_complex* out,int n,int direction)
{
	fftwf_plan p = fftwf_plan_dft_1d(n, in, out,(direction==1)?FFTW_FORWARD:FFTW_BACKWARD, FFTW_ESTIMATE);
	fftwf_execute(p);
	fftwf_destroy_plan(p);
}
void _stdcall MultipleFFT(void* input,void* output,int blocksize,int nblocks,int direction)
{
	fftwf_plan p=fftwf_plan_many_dft(1,//rank
									&blocksize,//n
									nblocks,//howmany
									(fftwf_complex*)input,//in
									NULL,//inembed
									1,//istride
									blocksize,//idist
									(fftwf_complex*)output,//out
									NULL,//onembed
									1,//ostride
									blocksize,//odist
									(direction==1)?FFTW_FORWARD:FFTW_BACKWARD,//sign
									FFTW_ESTIMATE//flags
									);
	fftwf_execute(p);
	fftwf_destroy_plan(p);
}
bool _stdcall Test()
{
	return LoadLibraryA("libfftw3f-3.dll")!=NULL;
}

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
					 )
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
		break;
	}
	return TRUE;
}

