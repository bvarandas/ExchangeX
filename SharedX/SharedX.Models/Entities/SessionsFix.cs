namespace SharedX.Core.Entities;
public class LoginFix
{
    public char EncryptMethod { get; set; }  //0= none / Other  99 =Custom
    public int HeartBtInt { get; set; } //Hearbeat interval in seconds
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool ResetSeqNumFlag { get; set; }  //Does client and server should reset sequence numbers

    public string DefaultApplVerID = string.Empty;

    /*
        8=FIXT.1.1|9=192|35=A|49=Target|56=EXBERRY|34=1|52=20230202-13:04:03.924000|98=0|108=30|141=Y|553=03189d54-ef96-4b21-909f-aaf746ba97c0|554=6342f2c079f30e8212345b5a7662f4ef6a45695186edc7e06d6cf9b123456789d6|1137=9|10=235|
        8=FIXT.1.1|9=82|35=A|49=EXBERRY|56=Target|34=1|52=20230202-13:04:04.359807|98=0|108=30|141=Y|1137=9|10=033|
    */
}

public class LogoutFix
{
    public string Text { get; set; } = string.Empty;
    /*
        8=FIXT.1.1|9=98|35=5|49=Target|56=EXBERRY|34=86|52=20230202-13:46:00.588000|58=Exit program. Message: SIGINT|789=92|10=176|
        8=FIXT.1.1|9=85|35=5|49=EXBERRY|56=Target|34=92|52=20230202-13:46:00.553453|58=Client initiated logout|10=099|
    */
}

public class HeartbeatFix
{
    public string TestReqID { get; set; } = string.Empty;
    /*
     * fixin 8=FIXT.1.1|9=57|35=0|49=EXBERRY|56=Target|34=7|52=20230202-13:05:20.655227|10=137|
    */
}

public class TestRequestFix
{
    public string TestReqID { get; set; } = string.Empty;
    /*
    8=FIXT.1.1|9=66|35=1|49=Target|56=EXBERRY|34=223|52=20230130-13:16:29.697000|112=19|10=044| 
    */
}

public class ResendRequestFix
{
    public ulong BeginSeqNo { get; set; }
    public ulong EndSeqNo { get; set; }
    /*
     *  8=FIXT.1.1|9=72|35=2|49=EXBERRY|56=Target|34=20906|52=20230130-10:57:31.965768|7=444|16=0|10=076|
     */
}

public class SequenceResetFix
{
    public char GapFillFlag { get; set; }
    public ulong NewSeqNo { get; set; }
    /*
     *  8=FIXT.1.1|9=72|35=4|49=Target|56=EXBERRY|34=448|52=20230130-10:57:31.963000|36=445|123=Y|10=097|
    */
}
public class RejectFix
{
    public ulong RefSeqNum { get; set; }
    public string Text { get; set; }= string.Empty;
    public int SessionRejectReason { get; set; }
    public int RefTagID { get; set; }
    public string RefMsgType { get; set; } = string.Empty;
    /*
     * 8=FIXT.1.1|9=82|35=3|49=EXBERRY|56=Target|34=2|52=20230202-14:04:51.618511|45=2|371=263|372=x|373=4|10=050|
    */
}