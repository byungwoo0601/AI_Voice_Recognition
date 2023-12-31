﻿#if UNITY_EDITOR || UNITY_ANDROID
using FrostweepGames.Plugins.Native;
using UnityEngine;

namespace AndroidRuntimePermissionsNamespace
{
	public class PermissionCallbackAsync : AndroidJavaProxy
	{
		private readonly string[] permissions;
		private readonly AndroidRuntimePermissions.PermissionResultMultiple callback;
		private readonly PermissionCallbackHelper callbackHelper;

		public PermissionCallbackAsync( string[] permissions, AndroidRuntimePermissions.PermissionResultMultiple callback ) : base( "com.yasirkula.unity.RuntimePermissionsReceiver" )
		{
			this.permissions = permissions;
			this.callback = callback;
			callbackHelper = new GameObject( "PermissionCallbackHelper" ).AddComponent<PermissionCallbackHelper>();
		}

		public void OnPermissionResult( string result )
		{
			callbackHelper.CallOnMainThread( () => ExecuteCallback( result ) );
		}

		private void ExecuteCallback( string result )
		{
			try
			{
				if( callback != null )
					callback( permissions, AndroidRuntimePermissions.ProcessPermissionRequest( permissions, result ) );
			}
			finally
			{
				Object.Destroy( callbackHelper.gameObject );
			}
		}
	}
}
#endif