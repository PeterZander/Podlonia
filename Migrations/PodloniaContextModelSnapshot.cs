﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Podlonia.Models;

namespace Podlonia.Migrations
{
    [DbContext(typeof(PodloniaContext))]
    partial class PodloniaContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.0");

            modelBuilder.Entity("Podlonia.Models.RSSEnclosure", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ContentType")
                        .HasColumnType("TEXT")
                        .HasColumnName("Type");

                    b.Property<long>("DownloadErrors")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("DownloadedAt")
                        .HasColumnType("TEXT");

                    b.Property<long>("DownloadsComplete")
                        .HasColumnType("INTEGER");

                    b.Property<long>("FeedId")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("ForceDownload")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("ForceSync")
                        .HasColumnType("INTEGER");

                    b.Property<long>("Length")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("PubDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("RssGuid")
                        .HasColumnType("TEXT");

                    b.Property<string>("Url")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Enclosures");
                });

            modelBuilder.Entity("Podlonia.Models.RSSFeed", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Category")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<long>("DownloadEnclosureMaxAgeDays")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ImageSource")
                        .HasColumnType("TEXT");

                    b.Property<string>("LocalPath")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<long>("StoredEnclosureMaxAgeDays")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Url")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Feeds");
                });

            modelBuilder.Entity("Podlonia.Models.SyncEnclosure", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long>("DeviceId")
                        .HasColumnType("INTEGER");

                    b.Property<long>("EncId")
                        .HasColumnType("INTEGER");

                    b.Property<long>("FeedId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("FullName")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("SyncEnclosures");
                });

            modelBuilder.Entity("Podlonia.Models.SyncUnit", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("MaxAgeDays")
                        .HasColumnType("INTEGER");

                    b.Property<int>("MaxFilesPerFeed")
                        .HasColumnType("INTEGER");

                    b.Property<long>("MaxStorageSpacePerFeed")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Path")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("SyncUnits");
                });
#pragma warning restore 612, 618
        }
    }
}
