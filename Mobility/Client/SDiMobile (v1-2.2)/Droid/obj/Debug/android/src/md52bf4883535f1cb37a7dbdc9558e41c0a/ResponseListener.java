package md52bf4883535f1cb37a7dbdc9558e41c0a;


public class ResponseListener
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		com.worklight.wlclient.api.WLResponseListener
{
	static final String __md_methods;
	static {
		__md_methods = 
			"n_onFailure:(Lcom/worklight/wlclient/api/WLFailResponse;)V:GetOnFailure_Lcom_worklight_wlclient_api_WLFailResponse_Handler:Worklight.Android.IWLResponseListenerInvoker, Worklight.Android\n" +
			"n_onSuccess:(Lcom/worklight/wlclient/api/WLResponse;)V:GetOnSuccess_Lcom_worklight_wlclient_api_WLResponse_Handler:Worklight.Android.IWLResponseListenerInvoker, Worklight.Android\n" +
			"";
		mono.android.Runtime.register ("Worklight.Xamarin.Android.ResponseListener, Worklight.Xamarin.Android, Version=6.3.0.0, Culture=neutral, PublicKeyToken=null", ResponseListener.class, __md_methods);
	}


	public ResponseListener () throws java.lang.Throwable
	{
		super ();
		if (getClass () == ResponseListener.class)
			mono.android.TypeManager.Activate ("Worklight.Xamarin.Android.ResponseListener, Worklight.Xamarin.Android, Version=6.3.0.0, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
	}


	public void onFailure (com.worklight.wlclient.api.WLFailResponse p0)
	{
		n_onFailure (p0);
	}

	private native void n_onFailure (com.worklight.wlclient.api.WLFailResponse p0);


	public void onSuccess (com.worklight.wlclient.api.WLResponse p0)
	{
		n_onSuccess (p0);
	}

	private native void n_onSuccess (com.worklight.wlclient.api.WLResponse p0);

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
