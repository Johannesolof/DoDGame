using System.Collections;
using System.Collections.Generic;

// The static MessageSystem class is to provide a global reach to subscribe 
// and register both listeners and feeders.
public static class MessageSystem
{
	static Dictionary<MessageFeeder, MessageFeeder> feeders  = new Dictionary<MessageFeeder, MessageFeeder>();

	static public void RegisterFeed(MessageFeeder feed)
	{
		feeders.Add(feed, feed);
	}
	static public void UnregisterFeed(MessageFeeder feed)
	{
		feeders.Remove(feed);
	}

	static public void SubscribeTo(string feedName, MessageSubscriber subs)
	{
		foreach(var f in feeders.Keys)
		{
			if(f.name == feedName)
			{
				f.Subscribe(subs);
			}
		}
	}
	static public void UnsubscribeFrom(string feedName, MessageSubscriber subs)
	{
		foreach(var f in feeders.Keys)
		{
			if(f.name == feedName)
			{
				f.Unsubscribe(subs);
			}
		}
	}
}

// Use this class whenever you want to broadcast a message, regardless
// of whether anyone cares
public class MessageFeeder
{
	public string name;
	Dictionary<MessageSubscriber, MessageSubscriber> subscribers;

	public MessageFeeder(string feedName) 
	{
		name = feedName;
		subscribers = new Dictionary<MessageSubscriber, MessageSubscriber>();
		MessageSystem.RegisterFeed(this);
	}

	public void Subscribe(MessageSubscriber subs)
	{
		subscribers.Add(subs, subs);
	}

	public void Unsubscribe(MessageSubscriber subs)
	{
		subscribers.Remove(subs);
	}

	public void Post(Message message)
	{
		foreach(var s in subscribers)
		{
			s.Value.OnPost(message);
		}
	}
}

// Derive from this class in order to be able to subscribe to specific feeds and
// receive updates from them
public abstract class MessageSubscriber
{
	public abstract void OnPost(Message message);

	public void SubscribeTo(string feedName)
	{
		MessageSystem.SubscribeTo(feedName, this);
	}
	public void UnsubscribeFrom(string feedName)
	{
		MessageSystem.UnsubscribeFrom(feedName, this);
	}
}

public enum MessageType
{
	// TODO: ADD entries here for different types of messages
}

// Derive from this message to define new message types, with specific 
// contents
public abstract class Message
{
	public Message(MessageType mtype) {type = mtype;}
	MessageType type;
}