# NSeetaface

## Library
[Seetaface](https://github.com/iarray/SeetaFaceEngine.Net) 
[Seetaface .NetDemo](https://github.com/iarray/SeetaFaceEngine.Net) 
[OpenCV 2.4.12](https://opencv.org/releases.html)

## Download the face models

* FaceAlignment:
		https://github.com/seetaface/SeetaFaceEngine/tree/master/FaceAlignment/model

* FaceDetection:
		https://github.com/seetaface/SeetaFaceEngine/tree/master/FaceDetection/model

* FaceIdentification:
		https://github.com/seetaface/SeetaFaceEngine/tree/master/FaceIdentification/model

## Use
Code Sample:
		
	string path = @"d:\model\";	//face models directory path
    SeetafaceHelper.SetModelDirectory(Encoding.Default.GetBytes(path));
    SeetafaceHelper.Init();


## running result
![Screenshot](result.jpg)