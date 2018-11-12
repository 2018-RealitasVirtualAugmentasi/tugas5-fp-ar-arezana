import sys, time
import mavlink_function as mav
from pymavlink import mavutil
from argparse import ArgumentParser

import socket
import sys

# Create a UDP socket
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

server_address = ('localhost', 10000)

class RAW_IMU:
    def __init__(self, _xacc, _yacc, _zacc, _xgyro, _ygyro, _zgyro, _xmag, _ymag, _zmag):
        self.xacc = _xacc
        self.yacc = _yacc
        self.zacc = _zacc
        self.xgyro = _xgyro
        self.ygyro = _ygyro
        self.zgyro = _zgyro
        self.xmag = _xmag
        self.ymag = _ymag
        self.zmag = _zmag

class RAW_GPS:
    def __init__(self, _fix_type, _lat, _lon, _alt, _eph, _epv, _vel, _cog, _satellites_visible):
        self.fix_type = _fix_type
        self.lat = _lat
        self.lon = _lon
        self.alt = _alt
        self.eph = _eph
        self.epv = _epv
        self.vel = _vel
        self.cog = _cog
        self.satellites_visible = _satellites_visible
    
master = mavutil.mavlink_connection("COM7", 115200)

heading = -999
roll = -999
pitch = -999
altitude = -999
latitude = -999
longitude = -999

GPS = RAW_GPS(0,0,0,0,0,0,0,0,0)
prev_GPS = RAW_GPS(0,0,0,0,0,0,0,0,0)
distance = 0
init = True

while True:
    try:
        master.mav.request_data_stream_send(master.target_system, master.target_component, 
		mavutil.mavlink.MAV_DATA_STREAM_ALL, 100, 1)
        # print(master.recv_match())
        message = master.recv_match()
        
        if not message:
            continue
        if message.get_type() == 'RAW_IMU': #ATTITUDE
            RAW_IMU_dict = message.to_dict()
            DATA = RAW_IMU(int(RAW_IMU_dict["xacc"]), int(RAW_IMU_dict["yacc"]), int(RAW_IMU_dict["zacc"]), int(RAW_IMU_dict["xgyro"]), int(RAW_IMU_dict["ygyro"]), int(RAW_IMU_dict["zgyro"]), int(RAW_IMU_dict["xmag"]), int(RAW_IMU_dict["ymag"]), int(RAW_IMU_dict["zmag"]))
            roll = mav.roll_estimate(DATA)
            pitch = mav.pitch_estimate(DATA)
            
        if message.get_type() == 'VFR_HUD':
            VFR_HUD_dict = message.to_dict()
            heading = int(VFR_HUD_dict["heading"])
            if(heading > 180):
                heading = heading - 360
            altitude = float(VFR_HUD_dict["alt"])

        if message.get_type() == 'GPS_RAW_INT':
            GPS_dict = message.to_dict()
            GPS = RAW_GPS(int(GPS_dict["fix_type"]), float(GPS_dict["lat"]) / 10000000, float(GPS_dict["lon"]) / 10000000, int(GPS_dict["alt"]), int(GPS_dict["eph"]), int(GPS_dict["epv"]), int(GPS_dict["vel"]), int(GPS_dict["cog"]), int(GPS_dict["satellites_visible"]))
            # if(not init):
            #     distance = mav.distance_two(GPS, prev_GPS) * 1000 #kilometer to meter
            
            # prev_GPS = GPS
            # if(prev_GPS.lon != 0 and prev_GPS.lat != 0):
            #     init = False
            
            # print("longitude: %f, latitude: %f, distance: %f" % (GPS.lon, GPS.lat, distance))
            
        if(heading != -999 and pitch != -999 and roll != -999 and altitude != -999 and GPS.lon != 0 and GPS.lat != 0):
            # print("heading: %d, pitch: %f, roll: %f, altitude: %f, longitude: %f, latitude: %f" % (heading, pitch, roll, altitude, GPS.lon, GPS.lat))
            # message = "AraTa:"+heading+":"+pitch+":"+roll+":"+altitude+":"+GPS.lon+":"+GPS.lat
            message = "AraTa:{0}:{1}:{2}:{3}:{4}:{5}".format(heading, pitch, roll, altitude, GPS.lon, GPS.lat)
            send_message = str.encode(message)
            print('sending {!r}'.format(send_message))
            sent = sock.sendto(send_message, server_address)
        
        # if message.get_type() == 'NAV_CONTROLLER_OUTPUT':
        #     print("message: %s" % message)
    except Exception as e:
        print(e)
        exit(0)

# Get some information !
# while True:
#     try:
#         print(master.recv_match().to_dict())
#     except:
#         pass
#     time.sleep(0.1)