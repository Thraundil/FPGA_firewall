#!/usr/bin/env python
import os
from test_functions import ipv4_in, ipv4_out, tcp_in, white, black

# Test description
print 'Test_3:'
print '''Tests that incoming \'TCP\' requests matching an IP in
         the state, must use the port associated with said state connection.
         As such, 2 of the incoming IPV4 requests should be accepted, but
         only 1 of the TCP requests should be accepted, as one of them uses
         a different port then that used in the saved state connection.'''

# IPV4 incoming (Src/Dest)
ipv4_incoming = [
                 ["0.0.0.0"    , "0.0.0.0"], # False
                 ["42.42.42.42", "1.1.1.1"], # True
                 ["42.42.42.42", "1.1.1.1"] # True
                ]

# TCP incoming (Src/Dest/Port/syn_ack_flag)
tcp_incoming  = [
                 ["0.0.0.0","0.0.0.0","0","0"], # False
                 ["42.42.42.42","1.1.1.1","80","1"], # False
                 ["42.42.42.42","1.1.1.1","42","1"], # False
                ]

# IPV4 outgoing (Src/Dest/port/syn_ack_flag)
ipv4_outgoing = [
                 ["1.1.1.1", "42.42.42.42", "42", "2"] # True
                ]

# SRC/DST ranges Whitelist
whitelist_src_dest = [
                      ["20.0.0.0 - 20.0.0.0" , "0.0.0.0 - 255.255.255.255"]
                     ]

# DST ranges Blacklist
blacklist = ["66.66.66.65-66.66.66.67"]



# *** Main ***
def main():

    # Opens/creates the files to write to
    ipv4_incoming_file = open("../input_data/ipv4_incoming.txt", "w")
    tcp_incoming_file  = open("../input_data/tcp_incoming.txt", "w")
    ipv4_outgoing_file = open("../input_data/ipv4_outgoing.txt", "w")
    whitelist_src  = open("../input_data/whitelist_src.txt", "w")
    whitelist_dest = open("../input_data/whitelist_dest.txt", "w")
    blacklist_dest = open("../input_data/blacklist.txt", "w")

    # Retrieves/calculates data to be written
    ipv4_incoming_new = ipv4_in(ipv4_incoming)
    tcp_incoming_new  = tcp_in(tcp_incoming)
    ipv4_outgoing_new = ipv4_out(ipv4_outgoing)
    white_src, white_dest = white(whitelist_src_dest)
    black_dest = black(blacklist)

    # Writes the data to file(s)
    ipv4_incoming_file.write(ipv4_incoming_new)
    tcp_incoming_file.write(tcp_incoming_new)
    ipv4_outgoing_file.write(ipv4_outgoing_new)
    whitelist_src.write(white_src)
    whitelist_dest.write(white_dest)
    blacklist_dest.write(black_dest)

    # Closes the files properly
    ipv4_incoming_file.close()
    tcp_incoming_file.close()
    ipv4_outgoing_file.close()
    whitelist_src.close()
    whitelist_dest.close()
    blacklist_dest.close()

    # Converts the file full of ascii bytes into 'actual' bytes
    os.system('xxd -p -r ../input_data/ipv4_incoming.txt > ../input_data/ipv4_incoming_bytes')
    os.system('xxd -p -r ../input_data/tcp_incoming.txt > ../input_data/tcp_incoming_bytes')
    os.system('xxd -p -r ../input_data/ipv4_outgoing.txt > ../input_data/ipv4_outgoing_bytes')
    os.system('rm ../input_data/ipv4_incoming.txt ../input_data/ipv4_outgoing.txt ../input_data/tcp_incoming.txt')

if __name__== "__main__":
    main()