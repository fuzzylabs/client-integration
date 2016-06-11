/*
 * Copyright (c) 2015 Google Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not
 * use this file except in compliance with the License. You may obtain a copy of
 * the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations under
 * the License.
 */

/**
 * Sample code to issue several basic Google Cloud Store (GCS) operations
 * using the Google Client Libraries.
 * For more information, see documentation for Compute Storage .NET client
 * https://cloud.google.com/storage/docs/json_api/v1/json-api-dotnet-samples
 */

using System;
using System.IO;
using System.Text;

using Google.Apis.Services;
using Google.Apis.Storage.v1;
using Google.Apis.Download;

namespace GoogleCloudSamples
{
    public class StorageSample
    {
		// [START create_storage_client]
		public StorageService CreateStorageClient(string pathToCredentials = "")
        {
            //var credentials = Google.Apis.Auth.OAuth2.GoogleCredential.GetApplicationDefaultAsync().Result;
			//Google.Apis.Auth.OAuth2.GoogleCredential.FromStream("")
			Google.Apis.Auth.OAuth2.GoogleCredential credentials;
			using (var stream = new FileStream(pathToCredentials, FileMode.Open, FileAccess.Read))
			{
				credentials = Google.Apis.Auth.OAuth2.GoogleCredential.FromStream(stream);
			}

            if (credentials.IsCreateScopedRequired)
            {
                credentials = credentials.CreateScoped(new[] { StorageService.Scope.DevstorageFullControl });
            }

            var serviceInitializer = new BaseClientService.Initializer()
            {
                ApplicationName = "Storage Sample",
                HttpClientInitializer = credentials
            };

            return new StorageService(serviceInitializer);
        }
        // [END create_storage_client]

        // [START list_buckets]
        public void ListBuckets(string projectId)
        {
            StorageService storage = CreateStorageClient();

            var buckets = storage.Buckets.List(projectId).Execute();

            if (buckets.Items != null)
            {
                foreach (var bucket in buckets.Items)
                {
                    Console.WriteLine($"Bucket: {bucket.Name}");
                }
            }
        }
        // [END list_buckets]

        // [START list_objects]
        public void ListObjects(string bucketName)
        {
            StorageService storage = CreateStorageClient();

            var objects = storage.Objects.List(bucketName).Execute();

            if (objects.Items != null)
            {
                foreach (var obj in objects.Items)
                {
                    Console.WriteLine($"Object: {obj.Name}");
                }
            }
        }
        // [END list_objects]

        // [START upload_stream]
		public void UploadStream(string pathToCredentials, string bucketName, string path, string objName)
        {
			Console.WriteLine(string.Format("Uploading file {0} in progress", path));
			StorageService storage = CreateStorageClient(pathToCredentials);
            using (var stream = new FileStream (path, FileMode.Open, FileAccess.Read)) {
				storage.Objects.Insert (
					bucket: bucketName,
					stream: stream,
					contentType: "text/plain",
					body: new Google.Apis.Storage.v1.Data.Object () { Name = objName }
				).Upload ();
			}

			Console.WriteLine(string.Format("Uploaded {0}", path));
        }
        // [END upload_stream]

        // [START download_stream]
        public void DownloadStream(string bucketName)
        {
            StorageService storage = CreateStorageClient();

            using (var stream = new MemoryStream())
            {
                storage.Objects.Get(bucketName, "my-file.txt").Download(stream);

                var content = Encoding.UTF8.GetString(stream.GetBuffer());

                Console.WriteLine($"Downloaded my-file.txt with content: {content}");
            }
        }
        // [END download_stream]

        // [START delete_object]
        public void DeleteObject(string bucketName)
        {
            StorageService storage = CreateStorageClient();

            storage.Objects.Delete(bucketName, "my-file.txt").Execute();

            Console.WriteLine("Deleted my-file.txt");
        }
        // [END delete_object]

        // [START media_downloader]
        public void DownloadToFile(string bucketName)
        {
            StorageService storage = CreateStorageClient();

            var objectToDownload = storage.Objects.Get(bucketName, "my-file.txt").Execute();

            var downloader = new MediaDownloader(storage);

            downloader.ProgressChanged += progress =>
            {
                Console.WriteLine($"{progress.Status} {progress.BytesDownloaded} bytes");
            };

            using (var fileStream = new FileStream("downloaded-file.txt", FileMode.Create))
            {
                var progress = downloader.Download(objectToDownload.MediaLink, fileStream);

                if (progress.Status == DownloadStatus.Completed)
                {
                    Console.WriteLine("Downloaded my-file.txt to downloaded-file.txt");
                }
            }
        }
        // [END media_downloader]
    }
}