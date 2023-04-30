terraform {
    required_providers {
        google-beta = {
        source  = "hashicorp/google-beta"
        version = "~> 4.0"
        }
    }
}

provider "google" {
    project = "<PROJECT_ID>"
    region  = "<REGION>"
    zone    = "<ZONE>"
}
