#!/usr/bin/env python
import os

# ***** TEST 0 *****

# IPV4 incoming (Src/Dest)
ipv4_incoming = [
                 ["11.0.0.0"     , "1.1.1.1"], # True
                 ["22.22.22.22"  , "1.1.1.1"], # False
                 ["9.255.255.255", "1.1.1.1"], # False
                 ["10.0.0.0"     , "1.1.1.1"], # True
                 ["20.0.0.0"     , "1.1.1.1"]  # True
                ]

# TCP incoming (Src/Dest/Port/syn_ack_flag)
tcp_incoming  = [
                ]

# IPV4 outgoing (Src/Dest/port/syn_ack_flag)
ipv4_outgoing = [
                ]

# SRC/DST ranges Whitelist
whitelist_src_dest = [
                      ["10.0.0.0 - 20.0.0.0" , "0.0.0.0 - 255.255.255.255"]
                     ]

# DST ranges Blacklist
blacklist = [
            ]



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
    ipv4_incoming_file = open("../input_data/ipv4_incoming.txt", "w")
    tcp_incoming_file  = open("../input_data/tcp_incoming.txt", "w")
    ipv4_outgoing_file = open("../input_data/ipv4_outgoing.txt", "w")
    whitelist_src  = open("../input_data/whitelist_src.txt", "w")
    whitelist_dest = open("../input_data/whitelist_dest.txt", "w")
    blacklist_dest = open("../input_data/blacklist.txt", "w")

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
    os.system('xxd -p -r ../input_data/ipv4_incoming.txt > ../input_data/ipv4_incoming_bytes')
    os.system('xxd -p -r ../input_data/tcp_incoming.txt > ../input_data/tcp_incoming_bytes')
    os.system('xxd -p -r ../input_data/ipv4_outgoing.txt > ../input_data/ipv4_outgoing_bytes')
    os.system('rm ../input_data/ipv4_incoming.txt ../input_data/ipv4_outgoing.txt ../input_data/tcp_incoming.txt')

if __name__== "__main__":
    main()