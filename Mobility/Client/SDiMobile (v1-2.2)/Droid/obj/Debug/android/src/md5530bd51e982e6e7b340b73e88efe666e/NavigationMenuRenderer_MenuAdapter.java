package md5530bd51e982e6e7b340b73e88efe666e;


public class NavigationMenuRenderer_MenuAdapter
	extends android.widget.BaseAdapter
	implements
		mono.android.IGCUserPeer
{
	static final String __md_methods;
	static {
		__md_methods = 
			"n_getView:(ILandroid/view/View;Landroid/view/ViewGroup;)Landroid/view/View;:GetGetView_ILandroid_view_View_Landroid_view_ViewGroup_Handler\n" +
			"n_getItemId:(I)J:GetGetItemId_IHandler\n" +
			"n_getCount:()I:GetGetCountHandler\n" +
			"n_getItem:(I)Ljava/lang/Object;:GetGetItem_IHandler\n" +
			"";
		mono.android.Runtime.register ("Xamarin.Forms.Platform.Android.NavigationMenuRenderer+MenuAdapter, Xamarin.Forms.Platform.Android, Version=1.4.0.0, Culture=neutral, PublicKeyToken=null", NavigationMenuRenderer_MenuAdapter.class, __md_methods);
	}


	public NavigationMenuRenderer_MenuAdapter () throws java.lang.Throwable
	{
		super ();
		if (getClass () == NavigationMenuRenderer_MenuAdapter.class)
			mono.android.TypeManager.Activate ("Xamarin.Forms.Platform.Android.NavigationMenuRenderer+MenuAdapter, Xamarin.Forms.Platform.Android, Version=1.4.0.0, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
	}


	public android.view.View getView (int p0, android.view.View p1, android.view.ViewGroup p2)
	{
		return n_getView (p0, p1, p2);
	}

	private native android.view.View n_getView (int p0, android.view.View p1, android.view.ViewGroup p2);


	public long getItemId (int p0)
	{
		return n_getItemId (p0);
	}

	private native long n_getItemId (int p0);


	public int getCount ()
	{
		return n_getCount ();
	}

	private native int n_getCount ();


	public java.lang.Object getItem (int p0)
	{
		return n_getItem (p0);
	}

	private native java.lang.Object n_getItem (int p0);

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
