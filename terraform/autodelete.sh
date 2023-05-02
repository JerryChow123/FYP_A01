#!/bin/bash
rm ../configure.ini
cd third
terraform destroy -auto-approve
cd ../second
terraform destroy -auto-approve
cd ../first
terraform destroy -auto-approve