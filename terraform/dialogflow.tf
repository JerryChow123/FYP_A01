
resource "google_project" "agent_project" {
    provider = google-beta
    project_id = "chatbot-<PROJECT_ID>"
    name = "chatbot-<PROJECT_ID>"
}

resource "google_project_service" "agent_project" {
    project = google_project.agent_project.project_id
    service = "dialogflow.googleapis.com"
    disable_dependent_services = false
}

resource "google_service_account" "dialogflow_service_account" {
    account_id = google_project.agent_project.project_id
}

resource "google_project_iam_member" "agent_create" {
    project = google_project_service.agent_project.project
    role    = "roles/dialogflow.admin"
    member  = "serviceAccount:${google_service_account.dialogflow_service_account.email}"
}

resource "google_dialogflow_agent" "full_agent" {
    project                  = google_project.agent_project.project_id
    display_name             = "dialogflow-cantonese"
    default_language_code    = "zh-hk"
    supported_language_codes = ["zh-hk"]
    time_zone                = "Asia/Hong_Kong"
    description              = "Dialogflow Cantonese"
    avatar_uri               = "https://cloud.google.com/_static/images/cloud/icons/favicons/onecloud/super_cloud.png"
    enable_logging           = true
    match_mode               = "MATCH_MODE_ML_ONLY"
    classification_threshold = 0.3
    api_version              = "API_VERSION_V2_BETA_1"
    tier                     = "TIER_STANDARD"

    depends_on = [
        google_project_service.agent_project
    ]
}

resource "google_service_account_key" "dialogflow" {
    service_account_id = google_service_account.dialogflow_service_account.id
    private_key_type   = "TYPE_GOOGLE_CREDENTIALS_FILE"
}

resource "local_file" "dialogflow_admin_key" {
    content  = nonsensitive(base64decode(google_service_account_key.dialogflow.private_key))
    filename = "dialogflow_admin_key.json"
}
