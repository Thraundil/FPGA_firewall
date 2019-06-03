#!/usr/bin/env python
import os

# IPV4 incoming (Src/Dest)
ipv4_incoming = [
                 ["130.226.237.173", "1.1.1.1"],
                 ["120.120.120.120", "1.1.1.1"],
                 ["2.2.2.2"        , "1.1.1.1"],
                 ["45.45.45.45"    , "1.1.1.1"],
                 ["91.224.211.80"  , "1.1.1.1"]
                ]

# TCP incoming (Src/Dest/Port/syn_ack_flag)
#   PLEASE NOTE, for testing purposes, ONLY include the above
#   ipv4_incoming addresses that WILL pass the whitelist/state.
tcp_incoming  = [
                 ["130.226.237.173", "1.1.1.1", "42", "2"],
                 ["120.120.120.120", "1.1.1.1", "42", "2"],
                 ["0.0.0.0", "0.0.0.0", "0", "0"], # To delay the tcp_incoming to match ipv4_incoming
                 ["0.0.0.0", "0.0.0.0", "0", "0"],
                 ["91.224.211.80"  , "1.1.1.1", "42", "2"]
                ]

# IPV4 outgoing (Src/Dest/port/syn_ack_flag)
ipv4_outgoing = [
                 ["1.1.1.1", "2.2.2.2"    , "42", "2"],
                 ["1.1.1.1", "66.66.66.66", "42", "2"],
                 ["1.1.1.1", "2.2.2.2"    , "42", "2"],
                ]

# SRC/DST ranges Whitelist
# Incoming IPV4 packeges SOURCE must be in either the whitelist or state.
whitelist_src_dest = [
                      ["130.226.237.173 - 130.226.237.175", "0.0.0.0 - 255.255.255.255"],
                      ["91.224.211.71   - 91.224.212.255" , "0.0.0.0 - 255.255.255.255"],
                      ["120.5.5.5       - 130.1.1.1"      , "0.0.0.0 - 255.255.255.255"],
                     ]

# DST ranges Blacklist
blacklist =          ["66.66.66.65-66.66.66.67"]


# ********* Please don't modify anything below this point **********

def ipv4_in():
    output = ""
    for x in ipv4_incoming:
        split0 = x[0].split('.')
        split1 = x[1].split('.')
        for y in (split0+split1):
            converted = str(hex(int(y)))
            if (len(str(hex(int(y)))) == 3):
                output += ('0' + converted)
            else:
                output += converted
        output += '\n'
    output = output.replace('0x','')
    return output


def ipv4_out():
    output = ""
    for x in ipv4_outgoing:
        split0 = x[0].split('.')
        split1 = x[1].split('.')
        for y in (split0+split1):
            converted = str(hex(int(y)))
            if (len(converted) == 3):
                output += ('0' + converted)
            else:
                output += converted

        port = str(hex(int(x[2])))
        flag = str(hex(int(x[3])))

        if (len(port) == 3):
            output += ('0' + port)
        else:
            output += port

        if (len(flag) == 3):
            output += ('0' + flag)
        else:
            output += flag
        output += '\n'
    output = output.replace('0x','')
    return output


def tcp_in():
    output = ""
    for x in tcp_incoming:
        split0 = x[0].split('.')
        split1 = x[1].split('.')
        for y in (split0+split1):
            converted = str(hex(int(y)))
            if (len(converted) == 3):
                output += ('0' + converted)
            else:
                output += converted

        port = str(hex(int(x[2])))
        flag = str(hex(int(x[3])))

        if (len(port) == 3):
            output += ('0' + port)
        else:
            output += port

        if (len(flag) == 3):
            output += ('0' + flag)
        else:
            output += flag
        output += '\n'
    output = output.replace('0x','')
    return output



def white():
    src, dest = "",""
    for x in whitelist_src_dest:
        src  += x[0].replace(' ','')
        src  += '\n'
        dest += x[1].replace(' ','')
        dest += '\n'
    return (src[:-1],dest[:-1])


def black():
    dest = ""
    for x in blacklist:
        dest += x.replace(' ','')
        dest += '\n'
    return (dest[:-1])


# *** Main ***
def main():

    # Opens/creates the files to write to
    ipv4_incoming_file = open("ipv4_incoming.txt", "w")
    tcp_incoming_file  = open("tcp_incoming.txt", "w")
    ipv4_outgoing_file = open("ipv4_outgoing.txt", "w")
    whitelist_src  = open("whitelist_src.txt", "w")
    whitelist_dest = open("whitelist_dest.txt", "w")
    blacklist_dest = open("blacklist.txt", "w")

    # Retrieves/calculates data to be written
    ipv4_incoming = ipv4_in()
    tcp_incoming  = tcp_in()
    ipv4_outgoing = ipv4_out()
    white_src, white_dest = white()
    black_dest = black()

    # Writes the data to file(s)
    ipv4_incoming_file.write(ipv4_incoming)
    tcp_incoming_file.write(tcp_incoming)
    ipv4_outgoing_file.write(ipv4_outgoing)
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
    os.system('xxd -p -r ipv4_incoming.txt > ipv4_incoming_bytes')
    os.system('xxd -p -r tcp_incoming.txt > tcp_incoming_bytes')
    os.system('xxd -p -r ipv4_outgoing.txt > ipv4_outgoing_bytes')
    os.system('rm ipv4_incoming.txt ipv4_outgoing.txt tcp_incoming.txt')

if __name__== "__main__":
    main()