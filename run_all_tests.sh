#!/usr/bin/env sh

# Change this to match the number of tests (0-indexed)
numberOfTests=7

touch "tests/output.txt"
echo "" > 'tests/output.txt'

var="$(seq -s ' ' 0 $numberOfTests)"
for i in $var
do
    cd "input_data"
    python "../tests/test_$i.py"
    echo ""
    cd ".."

    msbuild -nologo -verbosity:q
    cd "bin/Debug"
    echo "************** Test_$i Start ***************" >> '../../tests/output.txt'
    mono "sme_example.exe" >> '../../tests/output.txt'
    echo "************** Test Finished ****************" >> '../../tests/output.txt'
    cd '../..'
done

# Print results
cd 'tests'
python 'test_answers.py'
