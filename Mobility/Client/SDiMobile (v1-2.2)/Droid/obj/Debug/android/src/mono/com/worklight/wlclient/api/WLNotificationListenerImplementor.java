package mono.com.worklight.wlclient.api;


public class WLNotificationListenerImplementor
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		com.worklight.wlclient.api.WLNotificationListener
{
	static final String __md_methods;
	static {
		__md_methods = 
			"n_onMessage:(Ljava/lang/String;Ljava/lang/String;)V:GetOnMessage_Ljava_lang_String_Ljava_lang_String_Handler:Worklight.Android.IWLNotificationListenerInvoker, Worklight.Android\n" +
			"";
		mono.android.Runtime.register ("Worklight.Android.IWLNotificationListenerImplementor, Worklight.Android, Version=6.3.0.0, Culture=neutral, PublicKeyToken=null", WLNotificationListenerImplementor.class, __md_methods);
	}


	public WLNotificationListenerImplementor () throws java.lang.Throwable
	{
		super ();
		if (getClass () == WLNotificationListenerImplementor.class)
			mono.android.TypeManager.Activate ("Worklight.Android.IWLNotificationListenerImplementor, Worklight.Android, Version=6.3.0.0, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
	}


	public void onMessage (java.lang.String p0, java.lang.String p1)
	{
		n_onMessage (p0, p1);
	}

	private native void n_onMessage (java.lang.String p0, java.lang.String p1);

	java.util.ArrayList refList;
	public void monodroidAddReference (java.lang.Object obj)
	{
		if (refList == null)
			refList = new java.util.ArrayList ();
		refList.add (obj);
	}

	public void monodroidClearReferences ()
	{
		if (refList != null)
			refList.clear ();
	}
}
