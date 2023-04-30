#!/bin/bash
cd third
terraform destroy
cd ../second
terraform destroy
cd ../first
terraform destroy