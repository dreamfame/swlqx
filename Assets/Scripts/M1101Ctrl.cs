/** 
 *Copyright(C) 2018 by Orient Information Technology Co.,Ltd
 *All rights reserved. 
 *FileName:     	   	M1101Ctrl
 *Author:       	   	汪训泽
 *Date:         	   	2018/5/16 11:06:42 
 *Description: 		   	功能描述或者使用说明   
 *History: 				修改版本记录
*/
using System;
using System.Collections.Generic;
using System.IO.Ports;

public class M1101Ctrl
{
    private SerialPort comPort;

    public Action<bool> mResultAction = null;

    public bool ConnectCom(int comID = 3, int baudRate = 115200)
    {
        comPort = new SerialPort();
        comPort.PortName = "Com" + comID;
        comPort.ReceivedBytesThreshold = 1;
        comPort.BaudRate = baudRate;
        comPort.DataReceived += new SerialDataReceivedEventHandler(serialPort_DataReceived);
        comPort.Parity = Parity.Even;
        comPort.DataBits = 8;
        comPort.StopBits = StopBits.One;
        try
        {
            comPort.Open();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return false;
        }
    }

    public void SendData()
    {
        if (!comPort.IsOpen) return;
        byte[] sendData = new byte[8];
        sendData[0] = 0x01;
        sendData[1] = 0x02;
        sendData[2] = 0x00;
        sendData[3] = 0x00;
        sendData[4] = 0x00;
        sendData[5] = 0x10;
        sendData[6] = 0x79;
        sendData[7] = 0xC6;
        comPort.Write(sendData, 0, 8);
    }

    public void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        try
        {
            if (comPort.IsOpen)
            {
                int count = comPort.BytesToRead;
                if (count == 8)
                {
                    byte[] readBuffer = new byte[count];
                    comPort.Read(readBuffer, 0, count);

                    string printStr = "";
                    for (int i = 0; i < count; i++)
                    {
                        printStr += readBuffer[i] + " ";
                    }
                    Console.WriteLine(printStr);
                    bool isOn = false;
                    if (readBuffer[4] == 0 && readBuffer[5] == 0)
                    {
                        isOn = false;
                    }
                    else
                    {
                        isOn = true;
                    }
                    if (mResultAction != null)
                    {
                        mResultAction(isOn);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}
