package md52bf4883535f1cb37a7dbdc9558e41c0a;


public abstract class BaseChallengeHandler
	extends com.worklight.wlclient.api.challengehandler.BaseChallengeHandler
	implements
		mono.android.IGCUserPeer
{
	static final String __md_methods;
	static {
		__md_methods = 
			"";
		mono.android.Runtime.register ("Worklight.Xamarin.Android.BaseChallengeHandler, Worklight.Xamarin.Android, Version=6.3.0.0, Culture=neutral, PublicKeyToken=null", BaseChallengeHandler.class, __md_methods);
	}


	public BaseChallengeHandler (java.lang.String p0) throws java.lang.Throwable
	{
		super (p0);
		if (getClass () == BaseChallengeHandler.class)
			mono.android.TypeManager.Activate ("Worklight.Xamarin.Android.BaseChallengeHandler, Worklight.Xamarin.Android, Version=6.3.0.0, Culture=neutral, PublicKeyToken=null", "System.String, mscorlib, Version=2.0.5.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e", this, new java.lang.Object[] { p0 });
	}

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
