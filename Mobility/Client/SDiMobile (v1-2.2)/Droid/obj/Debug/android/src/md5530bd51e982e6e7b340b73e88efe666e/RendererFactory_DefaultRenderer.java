package md5530bd51e982e6e7b340b73e88efe666e;


public class RendererFactory_DefaultRenderer
	extends md5530bd51e982e6e7b340b73e88efe666e.VisualElementRenderer_1
	implements
		mono.android.IGCUserPeer
{
	static final String __md_methods;
	static {
		__md_methods = 
			"";
		mono.android.Runtime.register ("Xamarin.Forms.Platform.Android.RendererFactory+DefaultRenderer, Xamarin.Forms.Platform.Android, Version=1.4.0.0, Culture=neutral, PublicKeyToken=null", RendererFactory_DefaultRenderer.class, __md_methods);
	}


	public RendererFactory_DefaultRenderer (android.content.Context p0, android.util.AttributeSet p1, int p2) throws java.lang.Throwable
	{
		super (p0, p1, p2);
		if (getClass () == RendererFactory_DefaultRenderer.class)
			mono.android.TypeManager.Activate ("Xamarin.Forms.Platform.Android.RendererFactory+DefaultRenderer, Xamarin.Forms.Platform.Android, Version=1.4.0.0, Culture=neutral, PublicKeyToken=null", "Android.Content.Context, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065:Android.Util.IAttributeSet, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065:System.Int32, mscorlib, Version=2.0.5.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e", this, new java.lang.Object[] { p0, p1, p2 });
	}


	public RendererFactory_DefaultRenderer (android.content.Context p0, android.util.AttributeSet p1) throws java.lang.Throwable
	{
		super (p0, p1);
		if (getClass () == RendererFactory_DefaultRenderer.class)
			mono.android.TypeManager.Activate ("Xamarin.Forms.Platform.Android.RendererFactory+DefaultRenderer, Xamarin.Forms.Platform.Android, Version=1.4.0.0, Culture=neutral, PublicKeyToken=null", "Android.Content.Context, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065:Android.Util.IAttributeSet, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065", this, new java.lang.Object[] { p0, p1 });
	}


	public RendererFactory_DefaultRenderer (android.content.Context p0) throws java.lang.Throwable
	{
		super (p0);
		if (getClass () == RendererFactory_DefaultRenderer.class)
			mono.android.TypeManager.Activate ("Xamarin.Forms.Platform.Android.RendererFactory+DefaultRenderer, Xamarin.Forms.Platform.Android, Version=1.4.0.0, Culture=neutral, PublicKeyToken=null", "Android.Content.Context, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065", this, new java.lang.Object[] { p0 });
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
