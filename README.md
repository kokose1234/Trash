# Trash
분리수거를 위한 쓰레기 이미지 분류

### 준비사항

1. 작업 디렉토리에 `Setting.json`이 있어야함.
2. `iOS 13` SDK
3. `.NET 5` SDK
4. `AWS` 계정



#### Setting.json 구조

~~~json
{
    "awsAccessKeyId": "AWS 액세스 Id",
    "awsSecretAccessKey": "AWS 액세스 키",
    "modelArn": "프로젝트 ARN",
    "projectArn": "모델 버전 ARN",
    "versionName": "모델 버전명", 
    "s3BucketName": "S3 버킷명", 
    "queueUrl": "SQS 대기열명"
}
~~~