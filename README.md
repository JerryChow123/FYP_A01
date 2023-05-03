FYP A01 Learning Escape Game Project

Auto build resource in GCP
-> Google Cloud Shell
```
git clone https://github.com/JerryChow123/FYP_A01.git
cd terraform
python3 autodeploy.py
```
Auto delete resource in GCP (after built)
```
./autodelete.sh
```
Deployment will create a configure.ini file, put it in same path with EscapeGame.exe
```
demo\EscapeGame.exe
demo\configure.ini
```

Game source
```
git clone -b game https://github.com/JerryChow123/FYP_A01.git
```

