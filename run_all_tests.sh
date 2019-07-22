#!/usr/bin/env sh

# *** Test_0 ***
cd "input_data"
echo "Generating test data..."
python "../tests/test_0.py"
echo "Test data generated! \n"
cd ".."

msbuild -nologo -verbosity:q
cd "bin/Debug"
echo "************** Test_0 Start ***************"
mono "sme_example.exe"
cd '../..'
echo "************** Test_0 Done ****************"


# # *** Test_1 ***
# cd "input_data"
# echo "Generating test data..."
# python "../tests/test_1.py"
# echo "Test data generated! \n"
# cd ".."
# 
# msbuild
# cd "bin/Debug"
# echo "************** C# Output ***************"
# mono "sme_example.exe"
# cd '../..'
# 
# 
# 
# # *** Test_2 ***
# cd "input_data"
# echo "Generating test data..."
# python "../tests/test_2.py"
# echo "Test data generated! \n"
# cd ".."
# 
# msbuild
# cd "bin/Debug"
# echo "************** C# Output ***************"
# mono "sme_example.exe"
# cd '../..'
