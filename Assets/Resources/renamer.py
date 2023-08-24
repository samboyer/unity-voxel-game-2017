import os

rootdir = __file__.replace('\\','/')
rootdir = rootdir[:rootdir.rfind("/")]

changed=0

for root, subdirs, files in os.walk(rootdir):
    for f in files:
        if f[-4:]==".vox":
            print(f)
            full = os.path.join(root, f)
            os.rename(full, full[:-3]+"bytes")
            
            changed+=1

print(str(changed)+" .vox files renamed to .bytes")

input("Press enter to exit")

            
            
