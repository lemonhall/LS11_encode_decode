using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Pub.Utils
{
    public class LS11
    {
        /// <summary>
        /// 解码
        /// </summary>
        /// <param name="strFilePath"></param>
        /// <returns></returns>
        public static byte[] Decode(string strFilePath)
        {
            List<byte> lstDeCon = new List<byte>();//解压后内容
            using (System.IO.FileStream ls11FileReader = new System.IO.FileStream(strFilePath, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite))
            {
                byte[] bytes16 = new byte[16];//头16字节 LS11格式
                ls11FileReader.Seek(0, 0);
                ls11FileReader.Read(bytes16, 0, bytes16.Length);
                byte[] bytesDic = new byte[256];//256位字典
                ls11FileReader.Read(bytesDic, 0, bytesDic.Length);
                byte[] bytesLen1 = new byte[4];//压缩后长度
                ls11FileReader.Read(bytesLen1, 0, bytesLen1.Length);
                Array.Reverse(bytesLen1);//反转bytes
                int intLen1 = System.BitConverter.ToInt32(bytesLen1, 0);
                byte[] bytesLen2 = new byte[4];//解压后长度
                ls11FileReader.Read(bytesLen2, 0, bytesLen2.Length);
                Array.Reverse(bytesLen2);//反转bytes
                int intLen2 = System.BitConverter.ToInt32(bytesLen2, 0);
                byte[] bytesConStart = new byte[4];//文件起始位置
                ls11FileReader.Read(bytesConStart, 0, bytesConStart.Length);
                Array.Reverse(bytesConStart);//反转bytes
                int intConStart = System.BitConverter.ToInt32(bytesConStart, 0);
                byte[] bytesZero = new byte[4];//4个0
                ls11FileReader.Read(bytesConStart, 0, bytesConStart.Length);
                byte[] bytesCon = new byte[intLen1];//压缩后的正文
                ls11FileReader.Read(bytesCon, 0, bytesCon.Length);
                ls11FileReader.Close();
                //内容转成二进制
                StringBuilder sbConBinary = new StringBuilder();
                foreach (byte byteValue in bytesCon)
                {
                    sbConBinary.Append(Convert.ToString(byteValue, 2).PadLeft(8, '0'));//8位，不够前补0
                }
                string strConBinary = sbConBinary.ToString();

                #region 得到解压后索引列表
                int intPos = 0;
                List<int> lstDeConIndex = new List<int>();//解压后的索引
                List<char> lstTempSeg1 = new List<char>();//临时段1
                List<char> lstTempSeg2 = new List<char>();//临时段2
                while (intPos < strConBinary.Length)
                {
                    char charValue = strConBinary[intPos];
                    intPos++;
                    lstTempSeg1.Add(charValue);
                    if (charValue.Equals('0'))//遇0则段1结束
                    {
                        for (int i = 0; i < lstTempSeg1.Count; i++)//取出对应长度段2
                        {
                            lstTempSeg2.Add(strConBinary[intPos]);
                            intPos++;
                        }
                        String strSeg1 = new String(lstTempSeg1.ToArray());
                        String strSeg2 = new String(lstTempSeg2.ToArray());
                        int intItemValue = Convert.ToInt32(strSeg1, 2) + Convert.ToInt32(strSeg2, 2);
                        lstDeConIndex.Add(intItemValue);//段1+段2=实际值索引
                        lstTempSeg1.Clear();
                        lstTempSeg2.Clear();
                    }
                }
                #endregion

                #region 遍历索引列表，从字典获取实际值
                for (int i = 0; i < lstDeConIndex.Count; i++)
                {
                    if (lstDeCon.Count >= intLen2) break;
                    int intIndex = lstDeConIndex[i];
                    if (intIndex < 256)//字典范围内直接取值
                    {
                        lstDeCon.Add(bytesDic[intIndex]);
                        continue;
                    }
                    int intOffset = intIndex - 256;//大于255则向前复制取值
                    int intLen = lstDeConIndex[i + 1] + 3;//下一个索引值加3为取值长度
                    i++;
                    for (int j = 0; j < intLen; j++)
                    {
                        byte byteTemp = lstDeCon[lstDeCon.Count - intOffset];
                        lstDeCon.Add(byteTemp);
                    }
                }
                #endregion
            }
            return lstDeCon.ToArray();
        }

        /// <summary>
        /// 编码
        /// </summary>
        /// <param name="bytesCon"></param>
        /// <returns></returns>
        public static byte[] Encode(byte[] bytesCon)
        {
            byte[] bytesDic = new byte[256];//字典

            #region 统计字节使用频率,生成字典
            for (int i = 0; i <= 0xFF; i++)
            {
                bytesDic[i] = (byte)i;
            }
            int[] intFreq = new int[256];
            foreach (byte byteValue in bytesCon)
            {
                intFreq[byteValue]++;//出现次数累计
            }
            Array.Sort(intFreq, bytesDic);//按频率排序
            Array.Reverse(bytesDic);//倒序(从高到低)

            #endregion

            //Array.Sort(bytesDic);

            StringBuilder sbConBinary = new StringBuilder(); //索引号二进制

            #region 分解索引号二进制
            foreach (byte byteValue in bytesCon)
            {
                StringBuilder strSeg1 = new StringBuilder();//分解段1
                StringBuilder strSeg2 = new StringBuilder();//分解段2
                //获得内容索引号
                int intIndex = Array.IndexOf(bytesDic, byteValue);
                
                //分解索引号二进制
                string strBinary = Convert.ToString(intIndex, 2);
                if (Regex.IsMatch(strBinary, "^1+[        DISCUZ_CODE_0        ]quot;, RegexOptions.Multiline))//全是1，如111->110+001
                {
                    for (int i = 0; i < strBinary.Length - 1; i++)
                    {
                        strSeg1.Append("1");
                        strSeg2.Append("0");
                    }
                    strSeg1.Append("0");
                    strSeg2.Append("1");
                }
                else if (Regex.IsMatch(strBinary, "^1*0[        DISCUZ_CODE_0        ]quot;, RegexOptions.Multiline))//全是1结尾0，如1110->1110+0000
                {
                    strSeg1.Append(strBinary);
                    for (int i = 0; i < strBinary.Length; i++)
                    {
                        strSeg2.Append("0");
                    }
                }
                else //seg1为总长度-2个1，加一个0,减去seg1对应数值为seg2，如1001011->111110+001101
                {
                    for (int i = 0; i < strBinary.Length - 2; i++)
                    {
                        strSeg1.Append("1");
                    }
                    strSeg1.Append("0");
                    int intSeg1 = Convert.ToInt32(strSeg1.ToString(), 2);
                    int intSeg2 = intIndex - intSeg1;
                    strSeg2.Append(Convert.ToString(intSeg2, 2).PadLeft(strSeg1.Length,'0'));
                }
                
                sbConBinary.Append(strSeg1);
                sbConBinary.Append(strSeg2);
            }
            #endregion

            List<byte> lstEnConIndex = new List<byte>(); //压缩后的索引

            #region 将连续的二制转为bytes,得到压缩后的内容
            string strConBinary = sbConBinary.ToString();
            int intPos = 0;
            while (intPos < strConBinary.Length)
            {
                //8位一个字节
                StringBuilder strBinaryCon = new StringBuilder();

                for (int i = 0; i < 8; i++)
                {
                    if (intPos > strConBinary.Length - 1)
                    {
                        strBinaryCon.Append("0");
                    }
                    else
                    {
                        strBinaryCon.Append(strConBinary[intPos]);
                    }
                    intPos++;
                }
                lstEnConIndex.Add(Convert.ToByte(strBinaryCon.ToString(), 2));
            }
            #endregion

            List<byte> lstEnCon = new List<byte>(); //文件内容

            #region 组合文件全部内容
            //头16字节 LS11格式
            lstEnCon.AddRange(new byte[] { 0x4C, 0x53, 0x31, 0x31, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            //256位字典
            lstEnCon.AddRange(bytesDic);
            //内容压缩后长度
            byte[] bytesLen1 = BitConverter.GetBytes(lstEnConIndex.Count);
            Array.Reverse(bytesLen1);//反序
            lstEnCon.AddRange(bytesLen1);
            //内容解压后长度
            byte[] bytesLen2 = BitConverter.GetBytes(bytesCon.Length);
            Array.Reverse(bytesLen2);//反序
            lstEnCon.AddRange(bytesLen2);
            //文件起始位置
            lstEnCon.AddRange(new byte[] { 0x00, 0x00, 0x01, 0x20 });
            //4个0
            lstEnCon.AddRange(new byte[] { 0x00, 0x00, 0x00, 0x00 });
            //压缩后的正文
            lstEnCon.AddRange(lstEnConIndex);
            #endregion

            return lstEnCon.ToArray();
        }
    }
}