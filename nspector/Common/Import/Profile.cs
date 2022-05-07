namespace nspector.Common.Import;

[System.SerializableAttribute]
public class Profile
{
    public System.Collections.Generic.List<string> Executeables=new System.Collections.Generic.List<string>();
    public string                                  ProfileName ="";

    public System.Collections.Generic.List<ProfileSetting> Settings
        =new System.Collections.Generic.List<ProfileSetting>();
}