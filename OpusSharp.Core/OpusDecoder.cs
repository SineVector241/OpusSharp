﻿using OpusSharp.Enums;
using OpusSharp.SafeHandlers;
using System;

namespace OpusSharp
{
    /// <summary>
    /// Audio decoder with opus.
    /// </summary>
    public class OpusDecoder : IDisposable
    {
        private readonly OpusDecoderSafeHandle Decoder;

        #region Variables
        /// <summary>
        /// Sampling rate of input signal (Hz) This must be one of 8000, 12000, 16000, 24000, or 48000.
        /// </summary>
        public int SampleRate { get; }
        /// <summary>
        /// Number of channels (1 or 2) in input signal.
        /// </summary>
        public int Channels { get; }
        /// <summary>
        /// Configures decoder gain adjustment.
        /// </summary>
        public int Gain
        {
            get
            {
                if (Decoder.IsClosed) return 0;
                DecoderCtl(Enums.DecoderCtl.GET_GAIN, out int value);
                return value;
            }
            set
            {
                if (Decoder.IsClosed) return;
                DecoderCtl(Enums.DecoderCtl.SET_GAIN, value);
            }
        }
        /// <summary>
        /// Gets the duration (in samples) of the last packet successfully decoded or concealed.
        /// </summary>
        public int LastPacketDuration
        {
            get
            {
                if (Decoder.IsClosed) return 0;
                DecoderCtl(Enums.DecoderCtl.GET_LAST_PACKET_DURATION, out int value);
                return value;
            }
        }
        /// <summary>
        /// Gets the pitch of the last decoded frame, if available.
        /// </summary>
        public int Pitch
        {
            get
            {
                if (Decoder.IsClosed) return 0;
                DecoderCtl(Enums.DecoderCtl.GET_PITCH, out int value);
                return value;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Creates and initializes an opus decoder.
        /// </summary>
        /// <param name="SampleRate">Sample rate to decode at (Hz). This must be one of 8000, 12000, 16000, 24000, or 48000.</param>
        /// <param name="Channels">Number of channels (1 or 2) to decode.</param>
        public OpusDecoder(int SampleRate, int Channels)
        {
            Decoder = NativeOpus.opus_decoder_create(SampleRate, Channels, out var Error);
            CheckError((int)Error);

            this.SampleRate = SampleRate;
            this.Channels = Channels;
        }

        /// <summary>
        /// Decodes an Opus packet.
        /// </summary>
        /// <param name="input">Input payload. Use a NULL pointer to indicate packet loss.</param>
        /// <param name="inputLength">Number of bytes in payload.</param>
        /// <param name="output">Output signal (interleaved if 2 channels). length is frame_size*channels*sizeof(short).</param>
        /// <param name="frame_size">Number of samples per channel of available space in pcm. If this is less than the maximum packet duration (120ms; 5760 for 48kHz), this function will not be capable of decoding some packets. In the case of PLC (data==NULL) or FEC (decode_fec=1), then frame_size needs to be exactly the duration of audio that is missing, otherwise the decoder will not be in the optimal state to decode the next incoming packet. For the PLC and FEC cases, frame_size must be a multiple of 2.5 ms.</param>
        /// <param name="decodeFEC">Flag (false or true) to request that any in-band forward error correction data be decoded. If no such data is available, the frame is decoded as if it were lost.</param>
        /// <param name="inputOffset">Offset to start reading in the input.</param>
        /// <param name="outputOffset">Offset to start writing in the output.</param>
        /// <returns>The length of the decoded packet on success or a negative error code (see Error codes) on failure.</returns>
        public unsafe int Decode(byte[] input, int inputLength, byte[] output, int frame_size, bool decodeFEC = false, int inputOffset = 0, int outputOffset = 0)
        {
            ThrowIfDisposed();

            int result = 0;
            fixed (byte* inPtr = input)
            fixed (byte* outPtr = output)
                result = NativeOpus.opus_decode(Decoder, inPtr + inputOffset, inputLength, outPtr + outputOffset, frame_size / 2, decodeFEC ? 1 : 0);
            CheckError(result);
            return result * sizeof(short) * Channels;
        }

        /// <summary>
        /// Decodes an Opus packet.
        /// </summary>
        /// <param name="input">Input payload. Use a NULL pointer to indicate packet loss.</param>
        /// <param name="inputLength">Number of bytes in payload.</param>
        /// <param name="output">Output signal (interleaved if 2 channels). length is frame_size*channels.</param>
        /// <param name="frame_size">Number of samples per channel of available space in pcm. If this is less than the maximum packet duration (120ms; 5760 for 48kHz), this function will not be capable of decoding some packets. In the case of PLC (data==NULL) or FEC (decode_fec=1), then frame_size needs to be exactly the duration of audio that is missing, otherwise the decoder will not be in the optimal state to decode the next incoming packet. For the PLC and FEC cases, frame_size must be a multiple of 2.5 ms.</param>
        /// <param name="decodeFEC">Flag (false or true) to request that any in-band forward error correction data be decoded. If no such data is available, the frame is decoded as if it were lost.</param>
        /// <param name="inputOffset">Offset to start reading in the input.</param>
        /// <param name="outputOffset">Offset to start writing in the output.</param>
        /// <returns>The length of the decoded packet on success or a negative error code (see Error codes) on failure.</returns>
        public unsafe int Decode(byte[] input, int inputLength, short[] output, int frame_size, bool decodeFEC = false, int inputOffset = 0, int outputOffset = 0)
        {
            ThrowIfDisposed();

            byte[] byteOutput = new byte[output.Length * 2]; //Short to byte is 2 bytes.
            Buffer.BlockCopy(byteOutput, 0, byteOutput, 0, output.Length);

            int result = 0;
            fixed (byte* inPtr = input)
            fixed (byte* outPtr = byteOutput)
                result = NativeOpus.opus_decode(Decoder, inPtr + inputOffset, inputLength, outPtr + outputOffset, frame_size, decodeFEC ? 1 : 0);
            CheckError(result);
            Buffer.BlockCopy(byteOutput, 0, output, 0, output.Length);
            return result * Channels;
        }

        /// <summary>
        /// Decodes an Opus frame.
        /// </summary>
        /// <param name="input">Input in float format (interleaved if 2 channels), with a normal range of +/-1.0. Samples with a range beyond +/-1.0 are supported but will be clipped by decoders using the integer API and should only be used if it is known that the far end supports extended dynamic range. length is frame_size*channels*sizeof(float)</param>
        /// <param name="frame_size">Number of samples per channel in the input signal. This must be an Opus frame size for the encoder's sampling rate. For example, at 48 kHz the permitted values are 120, 240, 480, 960, 1920, and 2880. Passing in a duration of less than 10 ms (480 samples at 48 kHz) will prevent the encoder from using the LPC or hybrid modes.</param>
        /// <param name="output">Output payload</param>
        /// <param name="inputOffset">Offset to start reading in the input.</param>
        /// <param name="outputOffset">Offset to start writing in the output.</param>
        /// <returns>The length of the decoded packet on success or a negative error code (see Error codes) on failure.</returns>
        public unsafe int DecodeFloat(byte[] input, int inputLength, float[] output, int frame_size, bool decodeFEC = false, int inputOffset = 0, int outputOffset = 0)
        {
            ThrowIfDisposed();

            int result = 0;
            fixed (byte* inPtr = input)
            fixed (float* outPtr = output)
                result = NativeOpus.opus_decode_float(Decoder, inPtr + inputOffset, inputLength, outPtr + outputOffset, frame_size, decodeFEC ? 1 : 0);
            CheckError(result);
            return result * Channels;
        }

        /// <summary>
        /// Requests a CTL on the decoder.
        /// </summary>
        /// <param name="ctl">The decoder CTL to request.</param>
        /// <param name="value">The value to input.</param>
        public void DecoderCtl(Enums.DecoderCtl ctl, int value)
        {
            ThrowIfDisposed();

            CheckError(NativeOpus.opus_decoder_ctl(Decoder, (int)ctl, value));
        }

        /// <summary>
        /// Requests a CTL on the decoder.
        /// </summary>
        /// <param name="ctl">The decoder CTL to request.</param>
        /// <param name="value">The value that is outputted from the CTL.</param>
        public void DecoderCtl(Enums.DecoderCtl ctl, out int value)
        {
            ThrowIfDisposed();

            CheckError(NativeOpus.opus_decoder_ctl(Decoder, (int)ctl, out int val));
            value = val;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (!Decoder.IsClosed)
                Decoder.Dispose();
        }

        private void ThrowIfDisposed()
        {
            if (Decoder.IsClosed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }
        #endregion

        ~OpusDecoder()
        {
            Dispose(false);
        }

        protected static void CheckError(int result)
        {
            if (result < 0)
                throw new Exception($"Opus Error: {(OpusError)result}");
        }
    }
}
