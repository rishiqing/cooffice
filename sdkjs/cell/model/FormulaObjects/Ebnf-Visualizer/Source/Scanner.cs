/*-------------------------------------------------------------------------
EBNF Visualizer
Copyright (c) 2005 Stefan Schoergenhumer, Markus Dopler
supported by Hanspeter Moessenboeck, University of Linz

This program is free software; you can redistribute it and/or modify it 
under the terms of the GNU General Public License as published by the 
Free Software Foundation; either version 2, or (at your option) any 
later version.

This program is distributed in the hope that it will be useful, but 
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License 
for more details.

You should have received a copy of the GNU General Public License along 
with this program; if not, write to the Free Software Foundation, Inc., 
59 Temple Place - Suite 330, Boston, MA 02111-1307, USA.


This class has been generated by the Compiler Generator Coco/R.
See: http://www.ssw.uni-linz.ac.at/Research/Projects/Coco/
-------------------------------------------------------------------------*/

using System;
using System.IO;
using System.Collections;
using System.Text;



public class Token {
	public int kind;    // token kind
	public int pos;     // token position in the source text (starting at 0)
	public int col;     // token column (starting at 0)
	public int line;    // token line (starting at 1)
	public string val;  // token value
	public Token next;  // AW 2003-03-07 Tokens are kept in linked list
}

public class Buffer {
	public const char EOF = (char)256;
	static byte[] buf;
	static int bufLen;
	static int pos;
	
	public static void Fill (Stream s) {
		bufLen = (int) s.Length;
		buf = new byte[bufLen];
		s.Read(buf, 0, bufLen); 
		pos = 0;
	}
	
	public static int Read () {
		if (pos < bufLen) return buf[pos++];
		else return EOF;                          /* pdt */
	}

	public static int Peek () {
		if (pos < bufLen) return buf[pos];
		else return EOF;                          /* pdt */
	}
	
	/* AW 2003-03-10 moved this from ParserGen.cs */
	public static string GetString (int beg, int end) {
		StringBuilder s = new StringBuilder(64);
		int oldPos = Buffer.Pos;
		Buffer.Pos = beg;
		while (beg < end) { s.Append((char)Buffer.Read()); beg++; }
		Buffer.Pos = oldPos;
		return s.ToString();
	}

	public static int Pos {
		get { return pos; }
		set {
			if (value < 0) pos = 0; 
			else if (value >= bufLen) pos = bufLen; 
			else pos = value;
		}
	}
}

public class Scanner {
	const char EOL = '\n';
	const int eofSym = 0; /* pdt */
	const int charSetSize = 256;
	const int maxT = 13;
	const int noSym = 13;
	static short[] start = {
	  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
	  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
	  0,  0,  2,  0,  0,  0,  0,  3, 10, 11,  0,  0,  0,  0,  8,  0,
	  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  7,  0,  0,
	  0,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,
	  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1, 12,  5, 13,  0,  1,
	  0,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,
	  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1, 14,  9, 15,  0,  0,
	  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
	  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
	  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
	  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
	  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
	  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
	  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
	  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
	  -1};


	static Token t;          // current token
	static char ch;          // current input character
	static int pos;          // column number of current character
	static int line;         // line number of current character
	static int lineStart;    // start position of current line
	static int oldEols;      // EOLs that appeared in a comment;
	static BitArray ignore;  // set of characters to be ignored by the scanner

	static Token tokens;     // the complete input token stream
	static Token pt;         // current peek token
	
	public static void Init (string fileName) {
		FileStream s = null;
		try {
			s = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
			Init(s);
		} catch (IOException) {
			EbnfForm.WriteLine("--- Cannot open file "+ fileName);
			//System.Environment.Exit(1);
		} finally {
			if (s != null) s.Close();
		}
	}
	
	public static void Init (Stream s) {
		Buffer.Fill(s);
		pos = -1; line = 1; lineStart = 0;
		oldEols = 0;
		NextCh();
		ignore = new BitArray(charSetSize+1);
		ignore[' '] = true;  // blanks are always white space
		ignore[9] = true; ignore[10] = true; ignore[13] = true; 
		//--- AW: fill token list
		tokens = new Token();  // first token is a dummy
		Token node = tokens;
		do {
			node.next = NextToken();
			node = node.next;
		} while (node.kind != eofSym);
		node.next = node;
		node.val = "EOF";
		t = pt = tokens;
	}
	
	static void NextCh() {
		if (oldEols > 0) { ch = EOL; oldEols--; } 
		else {
			ch = (char)Buffer.Read(); pos++;
			// replace isolated '\r' by '\n' in order to make
			// eol handling uniform across Windows, Unix and Mac
			if (ch == '\r' && Buffer.Peek() != '\n') ch = EOL;
			if (ch == EOL) { line++; lineStart = pos + 1; }
		}

	}



	static void CheckLiteral() {
		switch (t.val) {
			default: break;
		}
	}

	/* AW Scan() renamed to NextToken() */
	static Token NextToken() {
		while (ignore[ch]) NextCh();

		t = new Token();
		t.pos = pos; t.col = pos - lineStart + 1; t.line = line; 
		int state = start[ch];
		StringBuilder buf = new StringBuilder(16);
		buf.Append(ch); NextCh();
		
		switch (state) {
			case -1: { t.kind = eofSym; goto done; } // NextCh already done /* pdt */
			case 0: { t.kind = noSym; goto done; }   // NextCh already done
			case 1:
				if ((ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z')) {buf.Append(ch); NextCh(); goto case 1;}
				else {t.kind = 1; goto done;}
			case 2:
				if ((ch >= ' ' && ch <= '!' || ch >= '#' && ch <= '~')) {buf.Append(ch); NextCh(); goto case 2;}
				else if (ch == '"') {buf.Append(ch); NextCh(); goto case 4;}
				else {t.kind = noSym; goto done;}
			case 3:
				if ((ch >= ' ' && ch <= '&' || ch >= '(' && ch <= '~')) {buf.Append(ch); NextCh(); goto case 3;}
				else if (ch == 39) {buf.Append(ch); NextCh(); goto case 4;}
				else {t.kind = noSym; goto done;}
			case 4:
				{t.kind = 2; goto done;}
			case 5:
				if (ch == 'n') {buf.Append(ch); NextCh(); goto case 6;}
				else {t.kind = noSym; goto done;}
			case 6:
				{t.kind = 3; goto done;}
			case 7:
				{t.kind = 4; goto done;}
			case 8:
				{t.kind = 5; goto done;}
			case 9:
				{t.kind = 6; goto done;}
			case 10:
				{t.kind = 7; goto done;}
			case 11:
				{t.kind = 8; goto done;}
			case 12:
				{t.kind = 9; goto done;}
			case 13:
				{t.kind = 10; goto done;}
			case 14:
				{t.kind = 11; goto done;}
			case 15:
				{t.kind = 12; goto done;}

		}
		done: 
		t.val = buf.ToString();
		return t;
	}
	
	/* AW 2003-03-07 get the next token, move on and synch peek token with current */
	public static Token Scan () {
		t = pt = t.next;
		return t;
	}

	/* AW 2003-03-07 get the next token, ignore pragmas */
	public static Token Peek () {
		do {                      // skip pragmas while peeking
			pt = pt.next;
		} while (pt.kind > maxT);
		return pt;
	}
	
	/* AW 2003-03-11 to make sure peek start at current scan position */
	public static void ResetPeek () { pt = t; }

} // end Scanner

