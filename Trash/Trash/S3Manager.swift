//  Copyright 2021 Jonguk Kim
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

import AWSCore
import AWSS3

public final class S3Manager {
    static let shared = S3Manager()

    private let s3Client: AWSS3TransferUtility;

    private init() {
        let credentialsProvider = AWSStaticCredentialsProvider(accessKey: "secret", secretKey: "secret")
        let configuration = AWSServiceConfiguration(region: .APNortheast2, credentialsProvider: credentialsProvider);
        AWSServiceManager.default().defaultServiceConfiguration = configuration;
        s3Client = AWSS3TransferUtility.default()
    }

    public func uploadImage(image: UIImage) {
        let name = "\(Int(1000 * Date().timeIntervalSince1970)).jpeg"
        let data = image.scalePreservingAspectRatio(targetSize: CGSize(width: 500, height: 500)).jpegData(compressionQuality: 0.5)

        s3Client.uploadData(data!,bucket: "trash-image-storage", key: "images/\(name)", contentType: "image/jpeg", expression: AWSS3TransferUtilityUploadExpression())
    }
}
