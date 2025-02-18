package md52bf4883535f1cb37a7dbdc9558e41c0a;


public class OnReadyToSubscribeListener
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		com.worklight.wlclient.api.WLOnReadyToSubscribeListener
{
	static final String __md_methods;
	static {
		__md_methods = 
			"n_onReadyToSubscribe:()V:GetOnReadyToSubscribeHandler:Worklight.Android.IWLOnReadyToSubscribeListenerInvoker, Worklight.Android\n" +
			"";
		mono.android.Runtime.register ("Worklight.Xamarin.Android.OnReadyToSubscribeListener, Worklight.Xamarin.Android, Version=6.3.0.0, Culture=neutral, PublicKeyToken=null", OnReadyToSubscribeListener.class, __md_methods);
	}


	public OnReadyToSubscribeListener () throws java.lang.Throwable
	{
		super ();
		if (getClass () == OnReadyToSubscribeListener.class)
			mono.android.TypeManager.Activate ("Worklight.Xamarin.Android.OnReadyToSubscribeListener, Worklight.Xamarin.Android, Version=6.3.0.0, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
	}


	public void onReadyToSubscribe ()
	{
		n_onReadyToSubscribe ();
	}

	private native void n_onReadyToSubscribe ();

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
