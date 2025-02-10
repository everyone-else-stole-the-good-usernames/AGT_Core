using Riptide;
using OpenTK.Mathematics;

public static class MessageExtensions // this class allows us to extend the Riptide Message class
{
    public static Message AddVector3(this Message message, Vector3 value) // adds support for sending the OpenTK Vector3 type as 3 floats
    {
        message.AddFloat(value.X);
        message.AddFloat(value.Y);
        message.AddFloat(value.Z);
        return message;
    }

    public static Message AddVector3i(this Message message, Vector3i value) // adds support for sending the OpenTK Vector3i type as 3 ints
    {
        message.AddInt(value.X);
        message.AddInt(value.Y);
        message.AddInt(value.Z);
        return message;
    }

    public static Message AddVector3b(this Message message, Vector3i value) // adds support for sending the OpenTK Vector3i type as 3 bytes
    {
        message.AddByte((byte)value.X);
        message.AddByte((byte)value.Y);
        message.AddByte((byte)value.Z);
        return message;
    }

    public static Message AddVector2i(this Message message, Vector2i value) // adds support for sending the OpenTK Vector2i type as 2 ints
    {
        message.AddInt(value.X);
        message.AddInt(value.Y);
        return message;
    }

    public static Vector3 GetVector3(this Message message) // adds support for recieving the OpenTK Vector3 type from 3 floats
    {
        return new Vector3(message.GetFloat(), message.GetFloat(), message.GetFloat());
    }

    public static Vector3i GetVector3i(this Message message) // adds support for recieving the OpenTK Vector3i type from 3 ints
    {
        return (message.GetInt(), message.GetInt(), message.GetInt());
    }

    public static Vector2i GetVector2i(this Message message) // adds support for recieving the OpenTK Vector3i type from 3 ints
    {
        return (message.GetInt(), message.GetInt());
    }
}