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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Amazon;
using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using TrashClient.Data;

namespace TrashClient.Manager
{
    internal class RekognitionManager
    {
        private static readonly Lazy<RekognitionManager> Lazy = new(() => new RekognitionManager());
        private readonly AmazonRekognitionClient client;


        internal static RekognitionManager Instance => Lazy.Value;

        private RekognitionManager()
        {
            client = new AmazonRekognitionClient(Setting.Value.AwsAccessKeyId, Setting.Value.AwsSecretAccessKey, RegionEndpoint.APNortheast2);
        }

        internal async Task<List<CustomLabel>> DetectObjectAsync(string imagePath)
        {
            try
            {
                var result = await client.DetectCustomLabelsAsync(CreateRequest(imagePath));

                return result.CustomLabels.Count == 0 ? new List<CustomLabel>() : result.CustomLabels;
            }
            catch (ResourceNotReadyException)
            {
                MessageBox.Show("모델이 켜지지 않았습니다.");
                return null;
            }
        }

        internal async Task<bool> StartModelAsync()
        {
            try
            {
                if (await GetModelStateAsync() != "RUNNING")
                {
                    await client.StartProjectVersionAsync(new StartProjectVersionRequest
                        {MinInferenceUnits = 1, ProjectVersionArn = Setting.Value.ModelArn});

                    while (true)
                    {
                        if (await GetModelStateAsync() == "RUNNING")
                        {
                            return true;
                        }

                        await Task.Delay(3000);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return false;
            }

            return true;
        }

        internal async Task<bool> StopModelAsync()
        {
            try
            {
                if (await GetModelStateAsync() != "STOPPED")
                {
                    await client.StopProjectVersionAsync(new StopProjectVersionRequest {ProjectVersionArn = Setting.Value.ModelArn});

                    while (true)
                    {
                        if (await GetModelStateAsync() == "STOPPED")
                        {
                            return true;
                        }

                        await Task.Delay(3000);
                    }
                }
            }
            catch (ResourceInUseException)
            {
                MessageBox.Show("모델이 시작 중이거나 종료 중입니다.");
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return false;
            }

            return true;
        }

        internal async Task<string> GetModelStateAsync()
        {
            var result = await client.DescribeProjectVersionsAsync(new DescribeProjectVersionsRequest
                {ProjectArn = Setting.Value.ProjectArn, VersionNames = new List<string> {Setting.Value.VersionName}});

            return result.ProjectVersionDescriptions.FirstOrDefault()?.Status ?? "STOPPED";
        }

        internal async Task<string> GetModelAccuracy()
        {
            var result = await client.DescribeProjectVersionsAsync(new DescribeProjectVersionsRequest
                { ProjectArn = Setting.Value.ProjectArn, VersionNames = new List<string> { Setting.Value.VersionName } });

            return $"{result.ProjectVersionDescriptions.FirstOrDefault()?.EvaluationResult.F1Score * 100 ?? 0f:F2}%";
        }

        internal async Task<bool> WaitForModel()
        {
            try
            {
                while (true)
                {
                    var state = await GetModelStateAsync();

                    if (state == "RUNNING" | state == "STOPPED") return true;

                    await Task.Delay(1000);
                }
            }
            catch (AmazonRekognitionException)
            {
                return false;
            }
        }

        private static DetectCustomLabelsRequest CreateRequest(string imagePath)
        {
            if (imagePath == null)
            {
                throw new ArgumentNullException("imagePath", "이미지 경로가 null이였습니다.");
            }

            try
            {
                var image = new Image {Bytes = new MemoryStream(File.ReadAllBytes(imagePath))};
                return new DetectCustomLabelsRequest
                    {Image = image, MinConfidence = 70f, MaxResults = 5, ProjectVersionArn = Setting.Value.ModelArn};
            }
            catch (IOException)
            {
                MessageBox.Show($"{Path.GetFileName(imagePath)}을(를) 읽을 수 없습니다.");
                return null;
            }
        }
    }
}