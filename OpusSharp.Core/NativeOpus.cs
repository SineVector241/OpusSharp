﻿using OpusSharp.Enums;
using OpusSharp.SafeHandlers;
using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;

namespace OpusSharp
{
    internal static unsafe class NativeOpus
    {
#if __ANDROID__
        private const string DllName = "libopus.so";
#elif __IOS__
        private const string DllName = "__Internal__";
#elif __MACCATALYST__
        private const string DllName = "__Internal__";
#else
        private const string DllName = "opus";
#endif
        #region Encoder
        [DllImport(DllName, EntryPoint = "opus_encoder_get_size", CallingConvention = CallingConvention.Cdecl)]
        public static extern int opus_encoder_get_size(int channels);

        [DllImport(DllName, EntryPoint = "opus_encoder_create", CallingConvention = CallingConvention.Cdecl)]
        public static extern OpusEncoderSafeHandle opus_encoder_create(int Fs, int channels, int application, out OpusError error);

        [DllImport(DllName, EntryPoint = "opus_encoder_init", CallingConvention = CallingConvention.Cdecl)]
        public static extern int opus_encoder_init(OpusEncoderSafeHandle st, int Fs, int channels, int application);

        [DllImport(DllName, EntryPoint = "opus_encode", CallingConvention = CallingConvention.Cdecl)]
        public static extern int opus_encode(OpusEncoderSafeHandle st, byte* pcm, int frame_size, byte* data, int max_data_bytes);

        [DllImport(DllName, EntryPoint = "opus_encode_float", CallingConvention = CallingConvention.Cdecl)]
        public static extern int opus_encode_float(OpusEncoderSafeHandle st, float* pcm, int frame_size, byte* data, int max_data_bytes);

        [DllImport(DllName, EntryPoint = "opus_encoder_destroy", CallingConvention = CallingConvention.Cdecl)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static extern void opus_encoder_destroy(IntPtr st);

        [DllImport(DllName, EntryPoint = "opus_encoder_ctl", CallingConvention = CallingConvention.Cdecl)]
        public static extern int opus_encoder_ctl(OpusEncoderSafeHandle st, int request, out int value);

        [DllImport(DllName, EntryPoint = "opus_encoder_ctl", CallingConvention = CallingConvention.Cdecl)]
        public static extern int opus_encoder_ctl(OpusEncoderSafeHandle st, int request, int value);
        #endregion

        #region Decoder
        [DllImport(DllName, EntryPoint = "opus_decoder_get_size", CallingConvention = CallingConvention.Cdecl)]
        public static extern int opus_decoder_get_size(int channels);

        [DllImport(DllName, EntryPoint = "opus_decoder_create", CallingConvention = CallingConvention.Cdecl)]
        public static extern OpusDecoderSafeHandle opus_decoder_create(int Fs, int channels, out OpusError error);

        [DllImport(DllName, EntryPoint = "opus_decoder_init", CallingConvention = CallingConvention.Cdecl)]
        public static extern int opus_decoder_init(OpusDecoderSafeHandle st, int Fs, int channels);

        [DllImport(DllName, EntryPoint = "opus_decode", CallingConvention = CallingConvention.Cdecl)]
        public static extern int opus_decode(OpusDecoderSafeHandle st, byte* data, int len, byte* pcm, int frame_size, int decode_fec);

        [DllImport(DllName, EntryPoint = "opus_decode_float", CallingConvention = CallingConvention.Cdecl)]
        public static extern int opus_decode_float(OpusDecoderSafeHandle st, byte* data, int len, float* pcm, int frame_size, int decode_fec);

        [DllImport(DllName, EntryPoint = "opus_decoder_ctl", CallingConvention = CallingConvention.Cdecl)]
        public static extern int opus_decoder_ctl(OpusDecoderSafeHandle st, int request, out int value);

        [DllImport(DllName, EntryPoint = "opus_decoder_ctl", CallingConvention = CallingConvention.Cdecl)]
        public static extern int opus_decoder_ctl(OpusDecoderSafeHandle st, int request, int value);

        [DllImport(DllName, EntryPoint = "opus_decoder_destroy", CallingConvention = CallingConvention.Cdecl)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static extern void opus_decoder_destroy(IntPtr st);

        [DllImport(DllName, EntryPoint = "opus_packet_parse", CallingConvention = CallingConvention.Cdecl)]
        public static extern int opus_packet_parse(byte* data, int len, byte* out_toc, byte* frames, short size, int* payload_offset);

        [DllImport(DllName, EntryPoint = "opus_packet_get_bandwidth", CallingConvention = CallingConvention.Cdecl)]
        public static extern int opus_packet_get_bandwidth(byte* data);

        [DllImport(DllName, EntryPoint = "opus_packet_get_samples_per_frame", CallingConvention = CallingConvention.Cdecl)]
        public static extern int opus_packet_get_samples_per_frame(byte* data, int Fs);

        [DllImport(DllName, EntryPoint = "opus_packet_get_nb_channels", CallingConvention = CallingConvention.Cdecl)]
        public static extern int opus_packet_nb_channels(byte* data);

        /* No Idea
        [DllImport(DllName, EntryPoint = "opus_packet_get_nb_frames", CallingConvention = CallingConvention.Cdecl)]
        public static extern int opus_packet_nb_frames(IntPtr data);
        */

        /* No Idea
        [DllImport(DllName, EntryPoint = "opus_packet_get_nb_samples", CallingConvention = CallingConvention.Cdecl)]
        public static extern int opus_packet_nb_samples(IntPtr data, int len);
        */

        /* No Idea
        [DllImport(DllName, EntryPoint = "opus_decoder_get_nb_samples", CallingConvention = CallingConvention.Cdecl)]
        public static extern int opus_packet_nb_samples(IntPtr dec, IntPtr packet, int len);
        */

        [DllImport(DllName, EntryPoint = "opus_pcm_soft_clip", CallingConvention = CallingConvention.Cdecl)]
        public static extern int opus_pcm_soft_clip(float* pcm, int frame_size, int channels, float* softclip_mem);
        #endregion
    }
}