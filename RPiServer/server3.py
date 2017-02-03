# server.py

def do_some_stuffs_with_input(input_string):

    print("Processing input...")
    input_string = input_string
    if (
        input_string == 'free'
    ):
        # print('green\n')
        return 'green'
        # strip.show()
        # colorWipe(strip, Color(0, 255, 0))  # Green wipe
    elif (
        input_string == 'busy' or
        input_string == 'on-the-phone' or
        input_string == 'in-presentation' or
        input_string == 'donotdisturb' or
        input_string == 'in-a-meeting' or
        input_string == 'in-a-conference'
    ):
        # print('red\n')
        return 'red'
        # strip.show()
        # colorWipe(strip, Color(255, 0, 0))  # Red wipe
    elif (
        input_string == 'berightback' or
        input_string == 'inactive' or
        input_string == 'away' or
        input_string == 'off-work'
    ):
        # print('yellow\n')
        return 'yellow'
        # strip.show()
        # colorWipe(strip, Color(255, 255, 0))  # Yellow wipe
    elif (
        input_string == 'testing'
    ):
        # print('testing\n')
        return input_string
    else:
        print('ignoring input_string\n')


    return input_string

def client_thread(conn, ip, port, MAX_BUFFER_SIZE = 1024):

    # the input is in bytes, so decode it
    input_from_client_bytes = conn.recv(MAX_BUFFER_SIZE)

    # MAX_BUFFER_SIZE is how big the message can be
    # this is test if it's sufficiently big
    import sys
    siz = sys.getsizeof(input_from_client_bytes)
    if  siz >= MAX_BUFFER_SIZE:
        print("The length of input is probably too long: {}".format(siz))

    # decode input and strip the end of line
    input_from_client = input_from_client_bytes.decode("utf8").rstrip()

    res = do_some_stuffs_with_input(input_from_client)
    print("Result of processing {} is: {}".format(input_from_client, res))

    vysl = res.encode("utf8")  # encode the result string
    conn.sendall(vysl)  # send it to client
    conn.close()  # close connection
    print('Connection ' + ip + ':' + port + " ended")

def start_server():

    import socket
    soc = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    # this is for easy starting/killing the app
    soc.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
    print('Socket created')

    try:
        soc.bind(("0.0.0.0", 5000))
        print('Socket bind complete')
    except socket.error as msg:
        import sys
        print('Bind failed. Error : ' + str(sys.exc_info()))
        sys.exit()

    #Start listening on socket
    soc.listen(10)
    print('Socket now listening')

    # for handling task in separate jobs we need threading
    from threading import Thread

    # this will make an infinite loop needed for 
    # not reseting server for every client
    while True:
        conn, addr = soc.accept()
        ip, port = str(addr[0]), str(addr[1])
        print('Accepting connection from ' + ip + ':' + port)
        try:
            Thread(target=client_thread, args=(conn, ip, port)).start()
        except:
            print("Terible error!")
            import traceback
            traceback.print_exc()
    soc.close()

start_server() 