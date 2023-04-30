
resource "google_storage_bucket" "static_site" {
    name          = "website-<PROJECT_ID>"
    location      = "asia-east2"
    force_destroy = true

    website {
        main_page_suffix = "index.html"
    }
}

resource "google_storage_bucket_iam_member" "allusers" {
    bucket = google_storage_bucket.static_site.name
    role   = "roles/storage.objectViewer"
    member = "allUsers"
}

variable "website_path" {
    type = string
    default = "../../static-website"
}

locals {
    my_files = fileset("${var.website_path}", "**")
}

resource "google_storage_bucket_object" "my_objects_html" {
    for_each = { for f in local.my_files : f => f if can(regex(".html", f)) }

    name   = each.value
    bucket = google_storage_bucket.static_site.name
    source = "${var.website_path}/${each.value}"
    content_type = "text/html"
}

resource "google_storage_bucket_object" "my_objects_js" {
    for_each = { for f in local.my_files : f => f if can(regex(".js", f)) }

    name   = each.value
    bucket = google_storage_bucket.static_site.name
    source = "${var.website_path}/${each.value}"
    content_type = "text/javascript"
}

resource "google_storage_bucket_object" "my_objects_css" {
    for_each = { for f in local.my_files : f => f if can(regex(".css", f)) }

    name   = each.value
    bucket = google_storage_bucket.static_site.name
    source = "${var.website_path}/${each.value}"
    content_type = "text/css"
}

resource "google_storage_bucket_object" "my_objects_other" {
    for_each = { for f in local.my_files : f => f if !can(regex(".html", f)) && !can(regex(".css", f)) && !can(regex(".js", f)) }

    name   = each.value
    bucket = google_storage_bucket.static_site.name
    source = "${var.website_path}/${each.value}"
}
