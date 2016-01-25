using UnityEngine;
using System.Collections;

public abstract class FSMState <T>
{
	public abstract void Enter (T entity);
	public abstract void Execute (T entity);
	public abstract void Exit (T entity);
}

public class FiniteStateMachine <T>
{
	private T _owner;
	private FSMState<T> _currentState = null;
	private FSMState<T> _previousState = null;
	private FSMState<T> _globalState = null;

	public bool isConfigured = false;

	public void Configure (T owner, FSMState<T> initialState)
	{
		_owner = owner;
		ChangeState (initialState);
		isConfigured = true;
	}

	public FSMState<T> CurrentState { get { return _currentState; } }

	public void ExecuteCurrentState()
	{
		Update();
	}

	public void Update ()
	{
		if (_globalState != null) _globalState.Execute(_owner);
		if (_currentState != null) _currentState.Execute(_owner);
	}

	public void ChangeState (FSMState<T> newState)
	{
		_previousState = _currentState;
		if (_currentState != null)
		{
			_currentState.Exit(_owner);
		}

		_currentState = newState;

		if (_currentState != null)
		{
			_currentState.Enter(_owner);
		}
	}

	public void RevertToPreviousState ()
	{
		if (_previousState != null)
		{
			ChangeState(_previousState);
		}
	}
}