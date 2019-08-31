#!/usr/bin/env sh

# IMPORTANT
# To test the benchmark (as this test is meant for), the simulator-files
# with the '_benchmark' extension must be swapped out with their counterparts!
# To check the number of clockcykles used, go to bin/Debug/output, open the trace.csv,
# and count the number of lines minus 2

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