def ipv4_in(ipv4_incoming):
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


def ipv4_out(ipv4_outgoing):
    output = ""
    for x in ipv4_outgoing:
#        tcp_udp = str(hex(int(x[0])))
#        if (tcp_udp == 3):
#            output += ('0' + tcp_udp)
#        else:
#            output += tcp_udp
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


def tcp_in(tcp_incoming):
    output = ""
    for x in tcp_incoming:
#        tcp_udp = str(hex(int(x[0])))
#        if (tcp_udp == 3):
#            output += ('0' + tcp_udp)
#        else:
#            output += tcp_udp
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



def white(whitelist_src_dest):
    src, dest = "",""
    for x in whitelist_src_dest:
        src  += x[0].replace(' ','')
        src  += '\n'
        dest += x[1].replace(' ','')
        dest += '\n'
    return (src[:-1],dest[:-1])


def black(blacklist):
    dest = ""
    for x in blacklist:
        dest += x.replace(' ','')
        dest += '\n'
    return (dest[:-1])
