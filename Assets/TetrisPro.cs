using UnityEngine;
using System.Collections;

public class TetrisPro : MonoBehaviour
{
	private const int MAX_ROW = 29;
	private const int MAX_COL = 10;
	private const int INIT_ROW = 2;
	private const int INIT_COL = 5;
	private const int BLOCK_WIDTH = 20;
	private const int BLOCK_HEIGHT = 20;
	private const int SCORE_STAGE = 1000;
	int row;
	int col;
	public GUISkin myskin = null;
	public GUISkin blockskin = null;
	public GUISkin effectskin = null;
	public GUISkin ghostskin = null;
	private static int[,] stateArray = new int[MAX_ROW + 1, MAX_COL + 2];
	float lastTime = 0.0f;
	float fallingSpeed = 1.0f;
	float startAnimationTime = 0.0f;
	int stage = 0;
	int score = 0;
	bool isInEffectAnimation = false;
	int countedFill = 0;
	private static int[] filledRows = new int[MAX_ROW];
	TetrisBlock curBlock;
	TetrisBlock nextBlock;
	TetrisBlock ghostBlock;
	
	void OnGUI ()
	{
		GUI.Label (new Rect (MAX_COL * BLOCK_WIDTH + 60, 30, 100, 30), "SCORE");
		GUI.Label (new Rect (MAX_COL * BLOCK_WIDTH + 60, 50, 100, 30), this.score.ToString ());
		GUI.Label (new Rect (MAX_COL * BLOCK_WIDTH + 60, 80, 100, 30), "NEXT");
		GUI.skin = myskin;
		for (row = 0; row < MAX_ROW; row ++) {
			GUI.Button (new Rect ((MAX_COL + 1) * BLOCK_WIDTH, BLOCK_HEIGHT * row, BLOCK_WIDTH, BLOCK_HEIGHT), "");
			GUI.Button (new Rect (0, BLOCK_HEIGHT * row, BLOCK_WIDTH, BLOCK_HEIGHT), "");
		}
		for (col = 0; col <= MAX_COL + 1; col ++) {
			GUI.Button (new Rect (col * BLOCK_WIDTH, MAX_ROW * BLOCK_HEIGHT, BLOCK_WIDTH, BLOCK_HEIGHT), "");
		}
		
		GUI.skin = blockskin;
		for (row = 0; row<MAX_ROW; row++) {
			for (col=1; col<=MAX_COL; col++) {
				if (stateArray [row, col] == 1) {
					GUI.Button (new Rect (col * BLOCK_WIDTH, row * BLOCK_HEIGHT, BLOCK_WIDTH, BLOCK_HEIGHT), "");
				}
			}
		}
		
		for (int i = 0; i < 4; i++) {
			int nextTmpRow = 6 + this.nextBlock.diffRow [i];
			int nextTmpCol = MAX_COL + 3 + this.nextBlock.diffCol [i];
			GUI.Button (new Rect (nextTmpCol * BLOCK_WIDTH, nextTmpRow * BLOCK_HEIGHT + 20, BLOCK_WIDTH, BLOCK_HEIGHT), "");
		}
		
		GUI.skin = effectskin;
		for (row = 0; row<MAX_ROW; row++) {
			for (col=1; col<=MAX_COL; col++) {
				if (stateArray [row, col] == 2) {
					GUI.Button (new Rect (col * BLOCK_WIDTH, row * BLOCK_HEIGHT, BLOCK_WIDTH, BLOCK_HEIGHT), "");
				}
			}
		}
		
		GUI.skin = ghostskin;
		for (row = 0; row<MAX_ROW; row++) {
			for (col=1; col<=MAX_COL; col++) {
				if (stateArray [row, col] == 3) {
					GUI.Button (new Rect (col * BLOCK_WIDTH, row * BLOCK_HEIGHT, BLOCK_WIDTH, BLOCK_HEIGHT), "");
				}
			}
		}
	}

	
	// Use this for initialization
	void Start ()
	{
		for (int i=0; i<MAX_ROW; i++) {
			stateArray [i, 0] = 1;
			stateArray [i, MAX_COL + 1] = 1;
		}
		for (int i=0; i<MAX_COL + 1; i++) {
			stateArray [MAX_ROW, i] = 1;
		}
		resetStateArray ();
		
		this.ghostBlock = new TetrisBlock (INIT_ROW, INIT_COL);
		
		this.nextBlock = new TetrisBlock (INIT_ROW, INIT_COL);
		GenNewBlock ();
		lastTime = Time.time;
		updateStateByBlock (curBlock, 1);
	}

	void moveBlock2Position (TetrisBlock tmpBlock, int toRow, int toCol)
	{
		
		updateStateByBlock (tmpBlock, 0);
		tmpBlock.centerRow = toRow;
		tmpBlock.centerCol = toCol;
		updateStateByBlock (tmpBlock, 1);
		
	}
	
	void updateStateByBlock (TetrisBlock tmpBlock, int stateVal)
	{
		int tmpRow;
		int tmpCol;
		Debug.Log ("center row:" + tmpBlock.centerRow + " center col:" + tmpBlock.centerCol);
		for (int i=0; i < tmpBlock.diffRow.GetLength(0); i++) {
			tmpRow = tmpBlock.centerRow + tmpBlock.diffRow [i];
			tmpCol = tmpBlock.centerCol + tmpBlock.diffCol [i];
			if (this.InField (tmpRow, tmpCol)) {
				stateArray [tmpRow, tmpCol] = stateVal;
			}
		}
	}
	// Update is called once per frame
	
	void Update ()
	{
		if (isInEffectAnimation == false) {
			TetrisMove ();
		} else {
			StartAnimationBlink ();
		}
		

		//Debug.Log ("Time:" + Time.time);
		StageManager ();

	}
	
	void TetrisMove ()
	{
		this.resetFilledRows ();
		this.resetGhostBlock ();
		int toRow = -1;
		int toCol = -1;
		bool isCollision = true;
		if (Input.GetKeyDown ("down")) {
			//toRow = curBlock.centerRow + 1;
			toRow = this.GetGroundedCenterRow (this.curBlock.centerRow, this.curBlock.centerCol);
			toCol = curBlock.centerCol;	
			Debug.Log ("down:" + isCollision);
		} else if (Input.GetKeyDown ("up")) {
			toRow = curBlock.centerRow;
			toCol = curBlock.centerCol;
			this.Rotate (toRow, toCol);
			Debug.Log ("up");
		} else if (Input.GetKeyDown ("left")) {
			toRow = curBlock.centerRow;
			toCol = curBlock.centerCol - 1;
			Debug.Log ("left");
		} else if (Input.GetKeyDown ("right")) {
			toRow = curBlock.centerRow;
			toCol = curBlock.centerCol + 1;		
			Debug.Log ("right");
		}
		
		//auto down set download speed
		if ((Time.time - lastTime - 1.5f * fallingSpeed) > 0) {
			toRow = curBlock.centerRow + 1;
			toCol = curBlock.centerCol;
			lastTime = Time.time;
		}
		
		
		//Move the block
		isCollision = this.IsCollision (toRow, toCol, curBlock);
		if (isCollision == false) {
			this.moveBlock2Position (curBlock, toRow, toCol);
		}
		
		if (IsGrounded ()) {
			//updateStateByBlock(this.ghostBlock, 0);
			this.countedFill = this.checkFilledLines ();
			if (this.countedFill > 0) {
				this.score += this.countedFill * this.countedFill * 100;
				this.isInEffectAnimation = true;
				startAnimationTime = Time.time;
			} else {
				this.countedFill = 0;

				GenNewBlock ();
			}
			
		} else {
			//updateStateByBlock(this.ghostBlock, 0);
			this.ghostBlock.centerRow = this.GetGroundedCenterRow (this.curBlock.centerRow, this.curBlock.centerCol);
			this.ghostBlock.centerCol = this.curBlock.centerCol;
			this.ghostBlock.diffRow = this.curBlock.diffRow;
			this.ghostBlock.diffCol = this.curBlock.diffCol;
			updateStateByBlock (this.ghostBlock, 3);
		}

	}

	void GenNewBlock ()
	{
		this.curBlock = this.nextBlock;
		this.nextBlock = new TetrisBlock (INIT_ROW, INIT_COL);
		checkGameOver ();
	}
	
	void checkGameOver ()
	{
		if (IsGameOver ()) {
			Application.LoadLevel ("StartMenu");
		}
	}

	bool IsGameOver ()
	{
		// when is grounded, check is it over game.
		bool gameOver = false;
		int tmpRow = -1;
		int tmpCol = -1;
		for (int i = 0; i < 4; i++) {
			tmpRow = this.curBlock.centerRow + this.curBlock.diffRow [i];
			tmpCol = this.curBlock.centerCol + this.curBlock.diffCol [i];
			if (stateArray [tmpRow, tmpCol] == 1) {
				gameOver = true;
			}
		}
		return gameOver;
	}

	void StartAnimationBlink ()
	{
		int flag = 1;
		float animationTime = Time.time - this.startAnimationTime;
		flag = isAnimationTimeBlink (animationTime) ? 2 : 1;
		for (int row = 0; row < filledRows.GetLength(0); row++) {
			if (filledRows [row] == 1) {
				SetStateArrayByRow (row, flag);
			}
		}
		//stop blink
		if (animationTime >= 1.5f) {
			Cascade ();
			if (checkFilledLines () == 0) {
				this.isInEffectAnimation = false;
				this.countedFill = 0;
				GenNewBlock ();
			} else {
				this.countedFill += this.checkFilledLines ();
				this.score += this.countedFill * this.countedFill * 100;
				this.isInEffectAnimation = true;
				startAnimationTime = Time.time;
			}
		}
		
	}

	void Cascade ()
	{
		int offset = 0;
		for (int row = MAX_ROW - 1; row > 1; row --) {
			if (filledRows [row] == 1) {
				CascadeMoveDown (row, offset);
				offset ++;
			}
		}
	}

	void CascadeMoveDown (int targetRow, int offset)
	{
		SetStateArrayByRow (targetRow + offset, 0);
		for (int row = targetRow + offset; row >= 1; row --) {
			for (int col = 1; col <= MAX_COL; col ++) {
				stateArray [row, col] = stateArray [row - 1, col];
			}
		}
	}

	bool isAnimationTimeBlink (float ani)
	{
		if ((ani > 0.0f && ani < 0.5f) || (ani > 1.0f && ani < 1.5f)) {
			return true;
		}
		return false;
	}

	void SetStateArrayByRow (int row, int val)
	{
		for (int col = 1; col <= MAX_COL; col++) {
			stateArray [row, col] = val;
		}
	}

	void StageManager ()
	{
		this.stage = (int)(this.score / SCORE_STAGE);
		this.fallingSpeed = 0.4f + 0.6f * 10 /(this.stage + 10);
	}

	bool IsFillingLine (int targetRow)
	{
		bool flag = true;
		for (int col = 1; col <= 10; col++) {
			if (stateArray [targetRow, col] == 0) {
				flag = false;
			}
		}
		return flag;
	}
	
	void Rotate (int targetRow, int targetCol)
	{
		int [] originalDiffRow = new int[4];
		int [] originalDiffCol = new int[4];
		//backup original position
		for (int i = 0; i < 4; i++) {
			originalDiffRow [i] = this.curBlock.diffRow [i];
			originalDiffCol [i] = this.curBlock.diffCol [i];
		}
		updateStateByBlock (this.curBlock, 0);
		int [] nextDiffRow = new int[4];
		int [] nextDiffCol = new int[4];
		
		for (int i = 0; i < 4; i++) {
			nextDiffRow [i] = originalDiffCol [i];
			nextDiffCol [i] = - originalDiffRow [i];
		}
		
		bool isCollision = false;
		int tmpRow;
		int tmpCol;
		for (int i = 0; i < 4; i++) {
			tmpRow = targetRow + nextDiffRow [i];
			tmpCol = targetCol + nextDiffCol [i];
			if (!InField (tmpRow, tmpCol) || (stateArray [tmpRow, tmpCol] == 1)) {
				isCollision = true;
			}
		}
		
		if (isCollision == false) {
			for (int i = 0; i < 4; i++) {
				this.curBlock.diffRow [i] = nextDiffRow [i];
				this.curBlock.diffCol [i] = nextDiffCol [i];
			}
		} else {
			for (int i = 0; i < 4; i++) {
				this.curBlock.diffRow [i] = originalDiffRow [i];
				this.curBlock.diffCol [i] = originalDiffCol [i];
			}
		}
		
		updateStateByBlock (this.curBlock, 1);
	}

	bool InField (int targetRow, int targetCol)
	{
		return (targetRow < MAX_ROW && targetRow >= 0 && targetCol >= 0 && targetCol <= MAX_COL + 1);
	}

	bool IsCollision (int targetRow, int targetCol, TetrisBlock tmpBlock)
	{
		int tmpRow;
		int tmpCol;
		bool result = false;
		updateStateByBlock (tmpBlock, 0);
		for (int i = 0; i < 4; i++) {
			tmpRow = targetRow + tmpBlock.diffRow [i];
			tmpCol = targetCol + tmpBlock.diffCol [i];
			if (!this.InField (tmpRow, tmpCol) || (stateArray [tmpRow, tmpCol] == 1)) {
				result = true;
			}
		}
		
		updateStateByBlock (tmpBlock, 1);
		return result;
	}

	bool IsGrounded ()
	{
		return this.IsCollision (this.curBlock.centerRow + 1, this.curBlock.centerCol, this.curBlock);
	}

	int GetGroundedCenterRow (int curRow, int curCol)
	{
		for (int targetRow = curRow; targetRow < MAX_ROW; targetRow ++) {
			if (this.IsCollision (targetRow + 1, curCol, this.curBlock))
				return targetRow;
		}
		return MAX_ROW - 1;
	}

	void resetFilledRows ()
	{
		for (int i = 0; i < filledRows.GetLength(0); i++) {
			filledRows [i] = -1;
		}
	}
	
	void resetGhostBlock ()
	{
		for (int row = 0; row < MAX_ROW; row ++) {
			for (int col = 1; col <=MAX_COL; col ++) {
				if (stateArray [row, col] == 3) {
					stateArray [row, col] = 0;
				}
			}
		}
	}
	
	void resetStateArray ()
	{
		for (int row = 0; row < MAX_ROW; row ++) {
			for (int col = 1; col <=MAX_COL; col ++) {	
				stateArray [row, col] = 0;
			}
		}
	}

	int checkFilledLines ()
	{
		int countLines = 0;
		for (int row = 1; row < MAX_ROW; row ++) {
			if (IsFillingLine (row)) {
				filledRows [row] = 1;
				countLines ++;
			}
		}
		return countLines;
	}
}

public class TetrisBlock
{
	int[,,] blockRelativePositions = new int[,,]{
	    {{0,0}, {1,0}, {2,0}, {-1,0}}, // I
	    {{0,0}, {0,1}, {1,0}, {1,1}}, // O
	    {{0,0}, {-1,0}, {0,1}, {1,1}}, // S
	    {{0,0}, {-1,1}, {0,1}, {1,0}}, // Z
	    {{0,0}, {-1,-1}, {0,-1}, {0,1}},    // J
	    {{0,0}, {-2,0}, {-1,0}, {0,1}}, // L
	    {{0,0}, {0,-1}, {0,1}, {1,0}}, // T
  	};
	public int[] diffRow = new int[4];
	public int[] diffCol = new int[4];
	public int centerRow;
	public int centerCol;
	
	public TetrisBlock (int centerRow1, int centerCol1)
	{
		this.centerRow = centerRow1;
		this.centerCol = centerCol1;
		int type = (int)(Random.value * blockRelativePositions.GetLength (0));
		//type = 0;
		for (int i = 0; i < blockRelativePositions.GetLength(1); i++) {
			this.diffRow [i] = this.blockRelativePositions [type, i, 0]; 
			this.diffCol [i] = this.blockRelativePositions [type, i, 1];
		}
	}
}
