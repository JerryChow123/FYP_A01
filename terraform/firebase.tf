
# Create a new Google Cloud project.
resource "google_project" "default" {
    provider = google-beta
    name       = "firebase-<PROJECT_ID>"
    project_id = "firebase-<PROJECT_ID>"

    # Required for any service that requires the Blaze pricing plan
    # (like Firebase Authentication with GCIP)
    # billing_account = "000000-000000-000000"

    # Required for the project to display in any list of Firebase projects.
    labels = {
        "firebase" = "enabled"
    }
}

# Enable Firebase services for the new project created above.
resource "google_firebase_project" "default" {
    provider = google-beta
    project = google_project.default.project_id
}

resource "google_service_account" "firebase" {
    account_id   = "firebase-admin"
    display_name = "Firebase Admin"
}

resource "google_project_iam_binding" "firebase_auth_admin" {
    project =  google_project.default.project_id
    role    = "roles/firebaseauth.admin"
    members = [
        "serviceAccount:${google_service_account.firebase.email}"
    ]
}

resource "google_project_iam_binding" "firebase_sdk_admin" {
    project =  google_project.default.project_id
    role    = "roles/firebase.sdkAdminServiceAgent"
    members = [
        "serviceAccount:${google_service_account.firebase.email}"
    ]
}

resource "google_project_iam_binding" "firebase_db_admin" {
    project =  google_project.default.project_id
    role    = "roles/firebasedatabase.admin"
    members = [
        "serviceAccount:${google_service_account.firebase.email}"
    ]
}

resource "google_project_service" "firebase_database" {
    provider = google-beta
    project  = google_firebase_project.default.project
    service  = "firebasedatabase.googleapis.com"
}

resource "google_firebase_database_instance" "default" {
    provider    = google-beta
    project     = google_firebase_project.default.project
    region      = "asia-southeast1"
    instance_id = "${google_project.default.project_id}-default-rtdb"
    type        = "DEFAULT_DATABASE"
    depends_on  = [google_project_service.firebase_database]
    desired_state   = "ACTIVE"
}

resource "google_service_account_key" "firebase" {
    service_account_id = google_service_account.firebase.id
    private_key_type   = "TYPE_GOOGLE_CREDENTIALS_FILE"
}

output "firebase_database_url" {
    value = "https://${google_project.default.project_id}-default-rtdb.${google_firebase_database_instance.default.region}.firebasedatabase.app/"
}

resource "local_file" "firebase_admin_key" {
    content  = nonsensitive(base64decode(google_service_account_key.firebase.private_key))
    filename = "firebase_admin_key.json"
}

resource "local_file" "firebase_database_url" {
    content  = "https://${google_project.default.project_id}-default-rtdb.${google_firebase_database_instance.default.region}.firebasedatabase.app/"
    filename = "firebase_database_url.txt"
}
