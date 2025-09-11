using System;
using System.Collections.Generic;

[Serializable]
public class ConversationData
{
    public string SceneId;
    public List<LineData> Lines;
}

[Serializable]
public class LineData
{
    public string Speaker;
    public string Expression;
    public string Side;
    public string Text;
}

