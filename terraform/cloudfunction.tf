
resource "google_storage_bucket" "bucket" {
    name     = "function-<PROJECT_ID>"
    location = "asia-east2"
}

resource "google_storage_bucket_object" "request-archive" {
    name   = "request.zip"
    bucket = google_storage_bucket.bucket.name
    source = "../../cloud-functions/request/request.zip"
}

resource "google_cloudfunctions_function" "request-function" {
    name        = "request"
    description = "request"
    runtime     = "python38"

    available_memory_mb          = 256
    source_archive_bucket        = google_storage_bucket.bucket.name
    source_archive_object        = google_storage_bucket_object.request-archive.name
    trigger_http                 = true
    timeout                      = 60
    entry_point                  = "web_response"
}

# IAM entry for all users to invoke the function
resource "google_cloudfunctions_function_iam_member" "invoker" {
    project        = google_cloudfunctions_function.request-function.project
    region         = google_cloudfunctions_function.request-function.region
    cloud_function = google_cloudfunctions_function.request-function.name

    role   = "roles/cloudfunctions.invoker"
    member = "allUsers"
}

resource "local_file" "request-function-url" {
    content  = google_cloudfunctions_function.request-function.https_trigger_url
    filename = "request-function-url.txt"
}
