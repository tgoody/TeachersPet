#include <iostream>
#include <vector>
#include <fstream>
#include <unistd.h>
using namespace std;

struct Image
{
    struct Header
    {
        char idLength;
        char colorMapType;
        char dataTypeCode;
        short colorMapOrigin;
        short colorMapLength;
        char colorMapDepth;
        short xOrigin;
        short yOrigin;
        short width = 0;
        short height = 0;
        char bitsPerPixel;
        char imageDescriptor;
        
        Header();
        // Read all of the header, and save info (constructor)
        // Header(string fileName);
    };
    
    struct Pixel
    {
        unsigned char b, g, r;
        Pixel();
        Pixel(unsigned char, unsigned char, unsigned char);
        Pixel operator*(const Pixel &d);
        Pixel operator+(const Pixel &d);
        Pixel operator-(const Pixel &d);
        Pixel ScreenOverload(Pixel, Pixel);
        Pixel Overlay(Pixel, Pixel);
    };
    
    Image();
    // Read the entire file and store that vector because this reads all the image
    Image(string fileName);
    vector<vector<Pixel>> imageInfo;
    Image::Header headerInfo;
};

Image::Header::Header()
{
	idLength = 0;
	colorMapType = 0;
	dataTypeCode = 0;
	colorMapOrigin = 0;
	colorMapLength = 0;
	colorMapDepth = 0;
	xOrigin = 0;
	yOrigin = 0;
	width = 0;
	height = 0;
	bitsPerPixel = 0;
	imageDescriptor = 0;
}


/*======================IMAGE========================*/
Image::Image()
{

}

Image::Image(string fileName)
{
    // Image::Header headerHW(fileName);
    // headerInfo = headerHW; // BETTER WAY OF DOING THIS!!!!!!!!!!!!!!!!!!!!!!!!

    ifstream someFile;
    someFile.open(fileName, ios_base::binary);
    
    if(!someFile.is_open()){
        
        cout << "######\nSHELL BREAK: " << fileName << "\n######" << endl;
        
    }
    
    if (someFile.is_open())
    {
        // Go through all of the data from the headerr))
        someFile.read((char *)&headerInfo.idLength, sizeof(headerInfo.idLength));
        someFile.read((char *)&headerInfo.colorMapType, sizeof(headerInfo.colorMapType));
        someFile.read((char *)&headerInfo.dataTypeCode, sizeof(headerInfo.dataTypeCode));
        someFile.read((char *)&headerInfo.colorMapOrigin, sizeof(headerInfo.colorMapOrigin));
        someFile.read((char *)&headerInfo.colorMapLength, sizeof(headerInfo.colorMapLength));
        someFile.read((char *)&headerInfo.colorMapDepth, sizeof(headerInfo.colorMapDepth));
        someFile.read((char *)&headerInfo.xOrigin, sizeof(headerInfo.xOrigin));
        someFile.read((char *)&headerInfo.yOrigin, sizeof(headerInfo.yOrigin));
        someFile.read((char *)&headerInfo.width, sizeof(headerInfo.width));
        someFile.read((char *)&headerInfo.height, sizeof(headerInfo.height));
        someFile.read((char *)&headerInfo.bitsPerPixel, sizeof(headerInfo.bitsPerPixel));
        someFile.read((char *)&headerInfo.imageDescriptor, sizeof(headerInfo.imageDescriptor));
        
        for (int i = 0; i < headerInfo.height; i++)
        {
            // Create a new row
            imageInfo.push_back(vector <Pixel>());
            
            for (int j = 0; j < headerInfo.width; j++)
            {
                // Read in the data into the 2D vector to have all pixels
                Image::Pixel tempPixel;
                someFile.read((char *)&tempPixel.b, 1);
                someFile.read((char *)&tempPixel.g, 1);
                someFile.read((char *)&tempPixel.r, 1);
                imageInfo[i].push_back(tempPixel);
            }
        }
        
        someFile.close();
    }
}

/*======================PIXEL========================*/
Image::Pixel::Pixel()
{
    this->b = 0;
    this->g = 0;
    this->r = 0;
}

Image::Pixel::Pixel(unsigned char R, unsigned char G, unsigned char B)
{
    this->b = B;
    this->g = G;
    this->r = R;
}

bool CheckingFunction(Image &myFile, string correctFile, int testNum);

int main(int argc, char** argv)
{
    
    chdir(argv[0]);
    int SCORE = 0;
    int EC = 0;
    int SCOREPERTASK = 9;
	int ECSCORE = 10;
    
    //Try to take in a score as a parameter
    if(argc > 1){
        try{
            SCOREPERTASK = stoi(argv[1]);
        }
        catch(invalid_argument& e){}
		
		if(argc > 2){
			try{
				ECSCORE = stoi(argv[2]);
			}
			catch(invalid_argument& e){}
		}
    }
    
    cout << "RUNNING TESTS" << endl;
    
    
    /*==================Part 1==================*/
    {
        Image output1("output/part1.tga");
        if(CheckingFunction(output1, "examples/EXAMPLE_part1.tga", 1))
        SCORE+=SCOREPERTASK;
    }

    /*==================Part 2==================*/
    {
        Image output2("output/part2.tga");
        
        if(CheckingFunction(output2, "examples/EXAMPLE_part2.tga", 2)){
            cout << "POSSIBLE CHEATER. PLEASE LOOK FURTHER INTO IT." << endl;
            return -1;
        }
        
        if(!CheckingFunction(output2, "examples/EXAMPLE_part2.tga", 2) &&
           CheckingFunction(output2, "examples/EXAMPLE_part2old.tga", 2))
        SCORE+=SCOREPERTASK;
        
        
        
        
    }

    /*==================Part 3==================*/
    {
        Image output3("output/part3.tga");
        if(CheckingFunction(output3, "examples/EXAMPLE_part3.tga", 3))
        SCORE+=SCOREPERTASK;
    }

    /*==================Part 4==================*/
    {
        Image output4("output/part4.tga");
        if(CheckingFunction(output4, "examples/EXAMPLE_part4.tga", 4))
        SCORE+=SCOREPERTASK;
    }

    /*==================Part 5==================*/
    {
        Image output5("output/part5.tga");
        if(CheckingFunction(output5, "examples/EXAMPLE_part5.tga", 5))
        SCORE+=SCOREPERTASK;
    }

    /*==================Part 6==================*/
    {
        Image output6("output/part6.tga");
        if(CheckingFunction(output6, "examples/EXAMPLE_part6.tga", 6))
        SCORE+=SCOREPERTASK;
    }

    /*==================Part 7==================*/
    {
        Image output7("output/part7.tga");
        if(CheckingFunction(output7, "examples/EXAMPLE_part7.tga", 7))
        SCORE+=SCOREPERTASK;
    }

    /*==================Part 8==================*/
    {
        Image carR("output/part8_r.tga");

        Image carG("output/part8_g.tga");

        Image carB("output/part8_b.tga");
        
        
        if(CheckingFunction(carR, "examples/EXAMPLE_part8_r.tga", 81) &&
           CheckingFunction(carG, "examples/EXAMPLE_part8_g.tga", 82) &&
           CheckingFunction(carB, "examples/EXAMPLE_part8_b.tga", 83))
        SCORE+=SCOREPERTASK;

    }

    /*==================Part 9==================*/
    {
        Image output9("output/part9.tga");
        if(CheckingFunction(output9, "examples/EXAMPLE_part9.tga", 9))
        SCORE+=SCOREPERTASK;
    }

    /*=================Part 10==================*/
    {
        Image output10("output/part10.tga");
        if(CheckingFunction(output10, "examples/EXAMPLE_part10.tga", 10))
        SCORE+=SCOREPERTASK;
    }

    /*================Extra Credit==============*/
    {
        Image outputEC("output/extraCredit.tga");
        if(CheckingFunction(outputEC, "examples/EXAMPLE_extraCredit.tga", 99999))
        EC=ECSCORE;
    }

    
    cout << "SCORE: " << SCORE << " EC: " << EC << endl;
    
    
    return 0;
}


bool CheckingFunction(Image &myFile, string correctFile, int testNum)
{
    Image correctImage(correctFile);
    
    cout << "TESTING: " << testNum << endl;

    if (myFile.headerInfo.height != correctImage.headerInfo.height ||
        myFile.headerInfo.width != correctImage.headerInfo.width)
       {
           if (testNum == 81)
                cout << "Test #8(r) failed at headerInfo." << endl;
           if (testNum == 82)
                cout << "Test #8(g) failed at headerInfo." << endl;
           if (testNum == 83)
                cout << "Test #8(b) failed at headerInfo." << endl;
           if (testNum == 100)
                cout << "Extra Credit failed at headerInfo." << endl;
           if (testNum < 20)
                cout << "Test #" << testNum << " failed at headerInfo." << endl;

           return false;
       }

    int failCount = 0;

    for (short i = 0; i < myFile.headerInfo.height; i++)
    {
        for (short j = 0; j < myFile.headerInfo.width; j++)
        {
            if (failCount >= 10)
            {
                if (testNum == 81)
                     cout << "Test #8(r) failed at least 10 times." << endl;
                if (testNum == 82)
                     cout << "Test #8(g) failed at least 10 times." << endl;
                if (testNum == 83)
                     cout << "Test #8(b) failed at least 10 times." << endl;
                if (testNum == 100)
                     cout << "Extra Credit failed at least 10 times." << endl;
                if (testNum < 20)
                    cout << "Test #" << testNum << " failed at least 10 times" << endl;

                return false;
            }

            if (myFile.imageInfo[i][j].b != correctImage.imageInfo[i][j].b ||
                myFile.imageInfo[i][j].g != correctImage.imageInfo[i][j].g ||
                myFile.imageInfo[i][j].r != correctImage.imageInfo[i][j].r)
            {
                if (testNum == 81)
                     cout << "Test #8(r)";
                if (testNum == 82)
                     cout << "Test #8(g)";
                if (testNum == 83)
                     cout << "Test #8(b)";
                if (testNum == 100)
                     cout << "Extra Credit";
                if (testNum < 20)
                    cout << "Test #" << testNum;

                cout << " failed at " << i << ", " << j << endl;

                if (myFile.imageInfo[i][j].b != correctImage.imageInfo[i][j].b)
                    cout << "Blue was wrong!" << endl;

                if (myFile.imageInfo[i][j].g != correctImage.imageInfo[i][j].g)
                    cout << "Green was wrong!" << endl;

                if (myFile.imageInfo[i][j].r != correctImage.imageInfo[i][j].r)
                    cout << "Red was wrong!" << endl;

                failCount++;
                return false;
            }
        }
    }

    if (failCount == 0)
    {
        if (testNum == 81)
            cout << "Test #8(r) passed!" << endl;

        if (testNum == 82)
            cout << "Test #8(g) passed!" << endl;

        if (testNum == 83)
            cout << "Test #8(b) passed!" << endl;

        if (testNum == 100)
            cout << "Extra Credit passed!" << endl;

        if (testNum < 20)
            cout << "Test #" << testNum << " passed!" << endl;
        return true;
    }
    
    return true;
}


// Make Files
// name of it: Makefile
// in terminal: /src~ make
    // this will run the Makefile
// $(Path) = "output"
// Every makefile has a set of rules and dependencies
// when a makefile is run, the rules are run like a function
// you have a rule that would just run the file
