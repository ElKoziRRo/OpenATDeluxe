// OpenATDmain.cpp: Definiert den Einstiegspunkt für die Konsolenanwendung.
//

#include "stdafx.h"

int main(int argc, char* argv[]) {
	//	MonoDomain *domain;
	int retval = 0;

	//FileManager::readfile("");
	//TEAKFILE *t = new TEAKFILE();
	//t->Open(".\room\abend.gli",0);
	//char *i = t->ReadLine();

	//main_function(domain, file, argc - 1, argv + 1);

	ATD_SDL *sdl = new ATD_SDL();
	sdl->OnExecute();



	system("pause");

	return retval;
}