
# Reads the test output
inputData = open('output.txt').readlines()
inputDataString = ""
for line in inputData:
    inputDataString += line

success_all = True

# For each Test
testData = ''.join(inputData).split('************** Test Finished ****************')
for x in range(0,len(testData)-1):

    print '******************** Starting Test_'+str(x)+' ********************'

    # Reads the expected/predicted output
    answers_ipv4_in  = open('test_'+str(x)+'_ipv4_in').readlines()
    answers_ipv4_out = open('test_'+str(x)+'_ipv4_out').readlines()
    answers_tcp_in   = open('test_'+str(x)+'_tcp_in').readlines()

    # The test data
    incoming_ipv4 = []
    incoming_tcp  = []
    outgoing_ipv4 = []


    # Updating the above lists with appropriate data
    lines = testData[x].split('\n')
    for y in lines:
        if 'Incoming IPV4' in y:
            incoming_ipv4.append(y)
        if 'Incoming TCP' in y:
            incoming_tcp.append(y)
        if 'Outgoing Package' in y:
            outgoing_ipv4.append(y)

    # The Success variable
    success = True
    for a,b in zip(incoming_ipv4, answers_ipv4_in):
        print a
        if a not in b:
            success = False

    for a,b in zip(incoming_tcp, answers_tcp_in):
        print a
        if a not in b:
            success = False

    for a,b in zip(outgoing_ipv4, answers_ipv4_out):
        print a
        if a not in b:
            success = False


    if success:
        print '******************** Test_'+str(x)+': Success ********************\n\n'
    else:
        print '******************** Test_'+str(x)+': FAILED ********************\n\n'
        success_all = False

if success_all:
    print '**************************************************************************************'
    print '*********************** SUCCESS - All tests were successfull! ************************'
    print '**************************************************************************************'
else:
    print '*******************************************************************************************'
    print '*************************** FAILURE - One or more tests failed! ***************************'
    print '*******************************************************************************************'