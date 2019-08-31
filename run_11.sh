#!/usr/bin/env sh

touch "tests/output.txt"
echo "" > 'tests/output.txt'

cd "input_data"
python "../tests/test_11.py"
echo ""
cd ".."

msbuild -nologo -verbosity:q
cd "bin/Debug"
echo "************** Test_11 Start ***************" >> '../../tests/output.txt'
mono "sme_example.exe" >> '../../tests/output.txt'
echo "************** Test Finished ****************" >> '../../tests/output.txt'
cd '../..'

# Print results
# cd 'tests'
# python 'test_answers.py'