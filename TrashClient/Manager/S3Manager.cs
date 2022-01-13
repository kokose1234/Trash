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

using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using TrashClient.Data;

namespace TrashClient.Manager
{
    internal class S3Manager
    {
        private static readonly Lazy<S3Manager> Lazy = new(() => new S3Manager());
        private readonly AmazonS3Client client;

        internal static S3Manager Instance => Lazy.Value;

        private S3Manager()
        {
            client = new AmazonS3Client(Setting.Value.AwsAccessKeyId, Setting.Value.AwsSecretAccessKey,
                new AmazonS3Config {RegionEndpoint = RegionEndpoint.APNortheast2});
        }

        private async Task DeleteFileAsync(string key)
        {
            var request = new DeleteObjectRequest {BucketName = Setting.Value.S3BucketName, Key = key};

            await client.DeleteObjectAsync(request);
        }

        internal async Task DownloadFileAsync(string key, string fileName)
        {
            var request = new GetObjectRequest {BucketName = Setting.Value.S3BucketName, Key = key};
            var response = await client.GetObjectAsync(request);

            await response.WriteResponseStreamToFileAsync(fileName, false, CancellationToken.None);
            await DeleteFileAsync(key);
        }
    }
}