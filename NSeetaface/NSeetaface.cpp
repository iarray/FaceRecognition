
#include <cstdint>
#include <fstream>
#include <iostream>
#include <string>
#include <tchar.h>

#include "cv.h"
#include "highgui.h"

#include "common.h"
#include "math.h"
#include "time.h"
#include "ctime"

#include "face_detection.h"
#include "face_alignment.h"
#include "face_identification.h"

#define	SEETAFACE_FEATURE_NUM 2048

typedef void(_stdcall *LogCallBack)(const char* logText);
typedef struct {
	seeta::FaceInfo face;
	seeta::FacialLandmark landmark[5];
} AlignmentResult;

std::string MODEL_DIR = "./model/";
LogCallBack logger = NULL;

seeta::FaceDetection detector(NULL);
seeta::FaceAlignment alignment(NULL);
seeta::FaceIdentification identification(NULL);

char* int2cstr(int num)
{
	char str[25];
	itoa(num, str, 10);
	return str;
}

std::string int2string(int num)
{
	char str[25];
	itoa(num, str, 10);
	return str;
}

void log(const char* txt)
{
	if (logger != NULL)
		logger(txt);
}

char * Str_Char(std::string &str)
{
	char *data;
	int len = str.length();
	data = (char *)malloc((len + 1) * sizeof(char));
	str.copy(data, len, 0);
	return data;
}

// 注册日志回调函数
extern "C" __declspec(dllexport) void SetDisplayLog(LogCallBack DisplayLog)
{
	logger = DisplayLog;
}

// 设置人脸模型目录
extern "C" __declspec(dllexport) void SetModelDirectory(const char* path)
{
	/*
	logger(path);
	int len = sizeof(path);
	logger(int2string(len));
	MODEL_DIR = new char[len];
	memset(MODEL_DIR, 0, len);
	strncpy(MODEL_DIR, path, len);
	*/

	MODEL_DIR = path;
	log(MODEL_DIR.c_str());
}

/*
 *	初始化人脸检测, 人脸对齐, 人脸识别
 *  初始化成功返回1
 */
extern "C" __declspec(dllexport) int Init()
{

	/*char* tDetectModelPath = strcat(MODEL_DIR ,"seeta_fd_frontal_v1.0.bin");

	char*  tAlignModelPath = strcat(MODEL_DIR , "seeta_fa_v1.1.bin");

	char* tIdentificationModelPath = strcat(MODEL_DIR , "seeta_fr_v1.0.bin");*/

	std::string tDetectModelPath = MODEL_DIR + "seeta_fd_frontal_v1.0.bin";
	log(tDetectModelPath.c_str());
	std::string  tAlignModelPath = MODEL_DIR + "seeta_fa_v1.1.bin";

	std::string tIdentificationModelPath = MODEL_DIR + "seeta_fr_v1.0.bin";

	FILE* fp = fopen(tDetectModelPath.c_str(), "r");
	if (!fp) {
		//人脸检测模型不存在
		log("人脸检测模型不存在");
		return -1;
	}
	fclose(fp);

	fp = fopen(tAlignModelPath.c_str(), "r");
	if (!fp) {
		//人脸对齐模型不存在
		log("人脸对齐模型不存在");
		return -2;
	}
	fclose(fp);


	fp = fopen(tIdentificationModelPath.c_str(), "r");
	if (!fp) {
		//人脸识别模型不存在
		log("人脸识别模型不存在");
		return -3;
	}
	fclose(fp);

	detector.initWithModel(tDetectModelPath.c_str());
	alignment.initWithModel(tAlignModelPath.c_str());
	identification.initWithModel(tIdentificationModelPath.c_str());
	
	return 1;
}

/*
 *	人脸检测
 */
extern "C" __declspec(dllexport) int DetectFace(const char* picPath, seeta::FaceInfo* face)
{
	//灰度图
	IplImage *img_grayscale = NULL;
	img_grayscale = cvLoadImage(picPath, 0);
	if (img_grayscale == NULL)
	{
		return 0;
	}
	//彩色图
	IplImage *img_color = cvLoadImage(picPath, 1);

	int im_width = img_grayscale->width;
	int im_height = img_grayscale->height;
	unsigned char* data = new unsigned char[im_width * im_height];
	unsigned char* data_ptr = data;
	unsigned char* image_data_ptr = (unsigned char*)img_grayscale->imageData;
	int h = 0;
	for (h = 0; h < im_height; h++) {
		memcpy(data_ptr, image_data_ptr, im_width);
		data_ptr += im_width;
		image_data_ptr += img_grayscale->widthStep;
	}

	seeta::ImageData image_data;
	image_data.data = data;
	image_data.width = im_width;
	image_data.height = im_height;
	image_data.num_channels = 1;

	// Detect faces
	std::vector<seeta::FaceInfo> faces = detector.Detect(image_data);
	size_t faceCount = faces.size();
	
	int32_t face_num = static_cast<int32_t>(faceCount);
	if (face_num == 0)
	{
		delete[]data;
		cvReleaseImage(&img_grayscale);
		cvReleaseImage(&img_color);
		return 0;
	}

	if (!faces.empty()) {
		face->bbox.x = faces[0].bbox.x;
		face->bbox.y = faces[0].bbox.y;
		face->bbox.height = faces[0].bbox.height;
		face->bbox.width = faces[0].bbox.width;
		face->pitch = faces[0].pitch;
		face->roll = faces[0].roll;
		face->score = faces[0].score;
		face->yaw = faces[0].yaw; 
	
		return faceCount;
	}
	 
	return 0;
}



/*
 *	人脸检测
 */
extern "C" __declspec(dllexport) int DetectFaces(const char* picPath, char* json)
{
	//灰度图
	IplImage *img_grayscale = NULL;
	img_grayscale = cvLoadImage(picPath, 0);
	if (img_grayscale == NULL)
	{
		return 0;
	}
	//彩色图
	IplImage *img_color = cvLoadImage(picPath, 1);

	int im_width = img_grayscale->width;
	int im_height = img_grayscale->height;
	unsigned char* data = new unsigned char[im_width * im_height];
	unsigned char* data_ptr = data;
	unsigned char* image_data_ptr = (unsigned char*)img_grayscale->imageData;
	int h = 0;
	for (h = 0; h < im_height; h++) {
		memcpy(data_ptr, image_data_ptr, im_width);
		data_ptr += im_width;
		image_data_ptr += img_grayscale->widthStep;
	}

	seeta::ImageData image_data;
	image_data.data = data;
	image_data.width = im_width;
	image_data.height = im_height;
	image_data.num_channels = 1;

	// Detect faces
	std::vector<seeta::FaceInfo> faces = detector.Detect(image_data);
	size_t faceCount = faces.size();
	size_t count = 0;
	std::string result = "[";

	int32_t face_num = static_cast<int32_t>(faceCount);
	if (face_num == 0)
	{
		delete[]data;
		cvReleaseImage(&img_grayscale);
		cvReleaseImage(&img_color);
		return 0;
	}

	if (!faces.empty()) {
		for (auto iter = faces.begin();iter != faces.end(); iter++, count++)
		{
			result += "{";
			result += "\"bbox\":{";
			result += ("\"x\":" + int2string((*iter).bbox.x) + ",");
			result += ("\"y\":" + int2string((*iter).bbox.y) + ",");
			result += ("\"height\":" + int2string((*iter).bbox.height) + ",");
			result += ("\"width\":" + int2string((*iter).bbox.width) + "}");
			result += (",\"pitch\":" + int2string((*iter).pitch));
			result += (",\"roll\":" + int2string((*iter).roll));
			result += (",\"score\":" + int2string((*iter).score));
			result += (",\"yaw\":" + int2string((*iter).yaw));
			result += "}";

			if (count < faceCount - 1)
				result += ",";

		}
		 
	}
	result += "]";
	log(result.c_str());

	strcpy(json, result.c_str());

	return faceCount;
 }


/*
 *	人脸对齐
 */
extern "C" __declspec(dllexport) int Alignment(const char* picPath, char* json)
{
	clock_t start, end = 0;

	//灰度图
	IplImage *img_grayscale = NULL;
	img_grayscale = cvLoadImage(picPath, 0);
	if (img_grayscale == NULL)
	{
		return 0;
	}
	//彩色图
	IplImage *img_color = cvLoadImage(picPath, 1);

	int im_width = img_grayscale->width;
	int im_height = img_grayscale->height;
	unsigned char* data = new unsigned char[im_width * im_height];
	unsigned char* data_ptr = data;
	unsigned char* image_data_ptr = (unsigned char*)img_grayscale->imageData;
	int h = 0;
	for (h = 0; h < im_height; h++) {
		memcpy(data_ptr, image_data_ptr, im_width);
		data_ptr += im_width;
		image_data_ptr += img_grayscale->widthStep;
	}

	seeta::ImageData image_data;
	image_data.data = data;
	image_data.width = im_width;
	image_data.height = im_height;
	image_data.num_channels = 1;

	start = clock();
	// Detect faces
	std::vector<seeta::FaceInfo> faces = detector.Detect(image_data);
	end = clock() - start;
	int haoshi = end;
	log(("检测人脸耗时:" + int2string(haoshi)).c_str());
	
	size_t faceCount = faces.size();

	int32_t face_num = static_cast<int32_t>(faceCount);
	if (face_num == 0)
	{
		delete[]data;
		cvReleaseImage(&img_grayscale);
		cvReleaseImage(&img_color);
		return 0;
	}

	size_t count = 0;
	std::string result = "[";

	if (!faces.empty()) {
		start = clock();
		for (auto iter = faces.begin(); iter != faces.end(); iter++, count++)
		{
			// 检测五官坐标
			seeta::FacialLandmark points[5];
			alignment.PointDetectLandmarks(image_data, (*iter), points);

			result += "{";
				result += "\"face\":{";
					result += "\"bbox\":{";
					result += ("\"x\":" + int2string((*iter).bbox.x) + ",");
					result += ("\"y\":" + int2string((*iter).bbox.y) + ",");
					result += ("\"height\":" + int2string((*iter).bbox.height) + ",");
					result += ("\"width\":" + int2string((*iter).bbox.width) + "}");
					result += (",\"pitch\":" + int2string((*iter).pitch));
					result += (",\"roll\":" + int2string((*iter).roll));
					result += (",\"score\":" + int2string((*iter).score));
					result += (",\"yaw\":" + int2string((*iter).yaw));
				result += "},";
				result += "\"landmark\":[";
				for (int32_t i = 0; i < 5; ++i) {
					result += ("{\"x\":" + int2string(points[i].x) + ",");
					result += ("\"y\":" + int2string(points[i].y) + "}");
					if(i<4) result += ",";
				}
				result += "]";
			result += "}";

			if (count < faceCount - 1)
				result += ",";

		}
		end = clock() - start;
		haoshi = end;
		log(("检测五官+序列化JSON 耗时:" + int2string(haoshi)).c_str());
	}
	result += "]";
	log(result.c_str());

	strcpy(json, result.c_str());

	return faceCount;
}

/*
 *	提取人脸特征
 */
extern "C" __declspec(dllexport) int ExtractFeature(
	const char* picPath,
	AlignmentResult* alignResult,
	float* feat
	//seeta::FaceInfo* face,
	//seeta::FacialLandmark* pt5
)
{
	seeta::FaceIdentification& face_recognizer = identification;
	clock_t start, count = 0;

	//int feat_size = face_recognizer.feature_size();
	cv::Mat src_img_color = cv::imread(picPath, 1);
	//cv::Mat src_img_color = cvLoadImage(picPath, 1);
	if (src_img_color.data == nullptr) {
		log("Load image error!");
		return 0;
	}

	seeta::ImageData src_img_data_color(src_img_color.cols, src_img_color.rows, src_img_color.channels());
	src_img_data_color.data = src_img_color.data;

	//seeta::ImageData src_img_data_color(src_img_color.cols, src_img_color.rows, src_img_color.channels());
	//src_img_data_color.data = src_img_color.data;

	//裁剪出人脸
	/*cv::Mat dst_img(face_recognizer.crop_height(),
		face_recognizer.crop_width(),
		CV_8UC(face_recognizer.crop_channels()));
	
	seeta::ImageData dst_img_data(dst_img.cols, dst_img.rows, dst_img.channels());
	dst_img_data.data = dst_img.data;
	int tCropRet = face_recognizer.CropFace(src_img_data, pt5, dst_img_data);*/

	log((int2string(alignResult->landmark[0].x) + "," + int2string(alignResult->landmark[0].y)).c_str());
	//float feat1[SEETAFACE_FEATURE_NUM];
	//提取特征
	start = clock();
	face_recognizer.ExtractFeatureWithCrop(src_img_data_color, alignResult->landmark, feat);
	//memcpy(feat, feat1, SEETAFACE_FEATURE_NUM);
	count += clock() - start;
	int haoshi = count;
	log(("提取特征耗时:" + int2string(haoshi)).c_str());
	return 1;
}


/*
*	计算人脸相似度(人脸特征1, 人脸特征2)
*/
extern "C" __declspec(dllexport) double CalcSimilarity(
	float* feat1, float* feta2
)
{
	double tSim = identification.CalcSimilarity(feat1, feta2);
	//保留两位小数
	tSim = int(100 * tSim) / 100.0;
	return tSim;
}

