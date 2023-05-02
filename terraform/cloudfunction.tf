
resource "google_storage_bucket" "bucket" {
    name     = "function-<PROJECT_ID>"
    location = "asia-east2"
}

# request

resource "google_storage_bucket_object" "request-archive" {
    name   = "request.zip"
    bucket = google_storage_bucket.bucket.name
    source = "../../cloud-functions/request/request.zip"
}

resource "google_cloudfunctions_function" "request-function" {
    name        = "test-request"
    description = "test-request"
    runtime     = "python38"

    available_memory_mb          = 256
    source_archive_bucket        = google_storage_bucket.bucket.name
    source_archive_object        = google_storage_bucket_object.request-archive.name
    trigger_http                 = true
    timeout                      = 60
    entry_point                  = "web_response"
}

# IAM entry for all users to invoke the function
resource "google_cloudfunctions_function_iam_member" "request-invoker" {
    project        = google_cloudfunctions_function.request-function.project
    region         = google_cloudfunctions_function.request-function.region
    cloud_function = google_cloudfunctions_function.request-function.name

    role   = "roles/cloudfunctions.invoker"
    member = "allUsers"
}

resource "local_file" "request-function-url" {
    content  = google_cloudfunctions_function.request-function.https_trigger_url
    filename = "request_url.txt"
}

# speechtext

resource "google_storage_bucket_object" "speechtext-archive" {
    name   = "speechtext.zip"
    bucket = google_storage_bucket.bucket.name
    source = "../../cloud-functions/speechtext/speechtext.zip"
}

resource "google_cloudfunctions_function" "speechtext-function" {
    name        = "test-speechtext"
    description = "test-speechtext"
    runtime     = "python38"

    available_memory_mb          = 256
    source_archive_bucket        = google_storage_bucket.bucket.name
    source_archive_object        = google_storage_bucket_object.speechtext-archive.name
    trigger_http                 = true
    timeout                      = 60
    entry_point                  = "speech_to_text"
}

# IAM entry for all users to invoke the function
resource "google_cloudfunctions_function_iam_member" "speechtext-invoker" {
    project        = google_cloudfunctions_function.speechtext-function.project
    region         = google_cloudfunctions_function.speechtext-function.region
    cloud_function = google_cloudfunctions_function.speechtext-function.name

    role   = "roles/cloudfunctions.invoker"
    member = "allUsers"
}

resource "local_file" "speechtext-function-url" {
    content  = google_cloudfunctions_function.speechtext-function.https_trigger_url
    filename = "speechtext_url.txt"
}

# textspeech

resource "google_storage_bucket_object" "textspeech-archive" {
    name   = "textspeech.zip"
    bucket = google_storage_bucket.bucket.name
    source = "../../cloud-functions/textspeech/textspeech.zip"
}

resource "google_cloudfunctions_function" "textspeech-function" {
    name        = "test-textspeech"
    description = "test-textspeech"
    runtime     = "python38"

    available_memory_mb          = 256
    source_archive_bucket        = google_storage_bucket.bucket.name
    source_archive_object        = google_storage_bucket_object.textspeech-archive.name
    trigger_http                 = true
    timeout                      = 60
    entry_point                  = "text_speech"
}

# IAM entry for all users to invoke the function
resource "google_cloudfunctions_function_iam_member" "textspeech-invoker" {
    project        = google_cloudfunctions_function.textspeech-function.project
    region         = google_cloudfunctions_function.textspeech-function.region
    cloud_function = google_cloudfunctions_function.textspeech-function.name

    role   = "roles/cloudfunctions.invoker"
    member = "allUsers"
}

resource "local_file" "textspeech-function-url" {
    content  = google_cloudfunctions_function.textspeech-function.https_trigger_url
    filename = "textspeech_url.txt"
}

# visionai

resource "google_storage_bucket_object" "visionai-archive" {
    name   = "visionai.zip"
    bucket = google_storage_bucket.bucket.name
    source = "../../cloud-functions/visionai/visionai.zip"
}

resource "google_cloudfunctions_function" "visionai-function" {
    name        = "test-visionai"
    description = "test-visionai"
    runtime     = "python38"

    available_memory_mb          = 256
    source_archive_bucket        = google_storage_bucket.bucket.name
    source_archive_object        = google_storage_bucket_object.visionai-archive.name
    trigger_http                 = true
    timeout                      = 60
    entry_point                  = "process_png"
}

# IAM entry for all users to invoke the function
resource "google_cloudfunctions_function_iam_member" "visionai-invoker" {
    project        = google_cloudfunctions_function.visionai-function.project
    region         = google_cloudfunctions_function.visionai-function.region
    cloud_function = google_cloudfunctions_function.visionai-function.name

    role   = "roles/cloudfunctions.invoker"
    member = "allUsers"
}

resource "local_file" "visionai-function-url" {
    content  = google_cloudfunctions_function.visionai-function.https_trigger_url
    filename = "visionai_url.txt"
}

# dialogflowbot

resource "google_storage_bucket_object" "dialogflowbot-archive" {
    name   = "dialogflowbot.zip"
    bucket = google_storage_bucket.bucket.name
    source = "../../cloud-functions/dialogflowbot/dialogflowbot.zip"
}

resource "google_cloudfunctions_function" "dialogflowbot-function" {
    name        = "test-dialogflowbot"
    description = "test-dialogflowbot"
    runtime     = "python38"

    available_memory_mb          = 256
    source_archive_bucket        = google_storage_bucket.bucket.name
    source_archive_object        = google_storage_bucket_object.dialogflowbot-archive.name
    trigger_http                 = true
    timeout                      = 60
    entry_point                  = "dialogflowbot"
}

# IAM entry for all users to invoke the function
resource "google_cloudfunctions_function_iam_member" "dialogflowbot-invoker" {
    project        = google_cloudfunctions_function.dialogflowbot-function.project
    region         = google_cloudfunctions_function.dialogflowbot-function.region
    cloud_function = google_cloudfunctions_function.dialogflowbot-function.name

    role   = "roles/cloudfunctions.invoker"
    member = "allUsers"
}

resource "local_file" "dialogflowbot-function-url" {
    content  = google_cloudfunctions_function.dialogflowbot-function.https_trigger_url
    filename = "dialogflowbot_url.txt"
}

